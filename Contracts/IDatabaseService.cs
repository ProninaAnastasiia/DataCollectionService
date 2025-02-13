using DataCollectionService.Messages;

namespace DataCollectionService.Contracts;

public interface IDatabaseService
{
    Task SaveCollectedData(string collectionToSave, string responseData);
}