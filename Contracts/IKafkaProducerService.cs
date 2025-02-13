using DataCollectionService.Messages;

namespace DataCollectionService.Contracts;

public interface IKafkaProducerService
{
    Task PublishDataCollectedMessage(DataCollectedMessage message);
}