using Confluent.Kafka;
using DataCollectionService.Contracts;
using DataCollectionService.Messages;

namespace DataCollectionService;

public class KafkaProducerService: IKafkaProducerService
{
    private readonly ILogger<KafkaProducerService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IProducer<Null, string> _producer;

    public KafkaProducerService(ILogger<KafkaProducerService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        
        var producerconfig = new ProducerConfig
        {
            BootstrapServers = _configuration["Kafka:BootstrapServers"]
        };

        _producer = new ProducerBuilder<Null, string>(producerconfig).Build();
    }

    public async Task ProduceAsync(string topic, string message)
    {
        _logger.LogInformation($"Publishing DataCollectedMessage...");
        var kafkamessage = new Message<Null, string> { Value = message, };

        await _producer.ProduceAsync(topic, kafkamessage);
    }
}