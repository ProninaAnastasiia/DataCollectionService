using MongoDB.Driver;

namespace DataCollectionService.Database;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;
    private readonly IConfiguration _configuration;

    public MongoDbContext(IConfiguration configuration)
    {
        _configuration = configuration;
        var client = new MongoClient(_configuration["MongoDb:ConnectionString"]);
        _database = client.GetDatabase(_configuration["MongoDb:DatabaseName"]);
    }

    public IMongoCollection<dynamic> GetResponseCollection(string collectionToSave)
    {
        var collectionName = _configuration[$"MongoDb:{collectionToSave}"];
        if (string.IsNullOrEmpty(collectionName))
        {
            throw new InvalidOperationException("MongoDb:CollectionName is not configured.");
        }

        return _database.GetCollection<dynamic>(collectionName);
    }
}