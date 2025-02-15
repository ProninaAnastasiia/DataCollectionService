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

            using (JsonDocument document = JsonDocument.Parse(responseData))
            {
                // Создаём пустой BsonDocument для записи в MongoDB
                var bsonDocument = new BsonDocument
                {
                    { "Timestamp", DateTime.UtcNow }
                };

                // Перебираем все свойства в корневом объекте
                foreach (JsonProperty property in document.RootElement.EnumerateObject())
                {
                    bsonDocument.Add(property.Name, ConvertJsonElementToBsonValue(property.Value));
                }

                await collection.InsertOneAsync(bsonDocument);
            }
        }
        catch (JsonException ex)
        {
            _logger.LogError($"Error deserializing response data: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error saving API response to MongoDB: {ex.Message}");
        }
    }
    private BsonValue ConvertJsonElementToBsonValue(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                var bsonDocument = new BsonDocument();
                foreach (JsonProperty property in element.EnumerateObject())
                {
                    bsonDocument.Add(property.Name, ConvertJsonElementToBsonValue(property.Value));
                }
                return bsonDocument;
            case JsonValueKind.Array:
                var bsonArray = new BsonArray();
                foreach (JsonElement arrayElement in element.EnumerateArray())
                {
                    bsonArray.Add(ConvertJsonElementToBsonValue(arrayElement));
                }
                return bsonArray;
            case JsonValueKind.String:
                return element.GetString();
            case JsonValueKind.Number:
                if (element.TryGetInt32(out int intValue))
                {
                    return intValue;
                }
                else if (element.TryGetInt64(out long longValue))
                {
                    return longValue;
                }
                else if (element.TryGetDouble(out double doubleValue))
                {
                    return doubleValue;
                }
                else
                {
                    return element.GetRawText(); // Fallback to string
                }
            case JsonValueKind.True:
                return true;
            case JsonValueKind.False:
                return false;
            case JsonValueKind.Null:
                return BsonNull.Value;
            default:
                return element.GetRawText(); //Fallback to string; should not happen
        }
    }
}