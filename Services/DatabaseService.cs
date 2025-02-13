using System.Text.Json;
using DataCollectionService.Contracts;
using DataCollectionService.Database;
using MongoDB.Bson;

namespace DataCollectionService;

public class DatabaseService: IDatabaseService
{
    private readonly ILogger<DatabaseService> _logger;
    private readonly MongoDbContext _dbContext;

    public DatabaseService(ILogger<DatabaseService> logger, MongoDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task SaveCollectedData(string collectionToSave, string responseData)
    {
        try
        {
            var collection = _dbContext.GetResponseCollection(collectionToSave);

            dynamic responseDocument;
            try
            {
                responseDocument = JsonSerializer.Deserialize<dynamic>(responseData);
            }
            catch (JsonException ex)
            {
                _logger.LogError($"Error deserializing response data: {ex.Message}");
                responseDocument = new { Error = "Failed to deserialize response", RawResponse = responseData };
            }

            // Создаём пустой BsonDocument для записи в MongoDB
            var bsonDocument = new BsonDocument
            {
                { "Timestamp", DateTime.UtcNow }
            };
            
            // Перебираем все ключи и значения в responseDocument
            var properties = (IDictionary<string, object>)responseDocument;
            foreach (var property in properties)
            {
                // Если свойство является вложенным объектом, рекурсивно преобразуем его в BsonDocument
                if (property.Value is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Object)
                {
                    bsonDocument.Add(property.Key, BsonDocument.Parse(jsonElement.ToString()));
                }
                else
                {
                    bsonDocument.Add(property.Key, BsonValue.Create(property.Value));
                }
            }

            await collection.InsertOneAsync(bsonDocument);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error saving API response to MongoDB: {ex.Message}");
        }
    }
    
}