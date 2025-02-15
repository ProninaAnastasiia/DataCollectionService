namespace DataCollectionService.Contracts;

public interface IKafkaProducerService
{
    Task ProduceAsync(string topic, string message);
}