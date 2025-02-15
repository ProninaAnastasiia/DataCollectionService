using System.Text.Json;
using Confluent.Kafka;
using DataCollectionService.Contracts;
using DataCollectionService.Messages;

namespace DataCollectionService;

public class KafkaConsumerService: BackgroundService
{
    private readonly ILogger<KafkaConsumerService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IMessageHandleService _messageMediator;

    public KafkaConsumerService(ILogger<KafkaConsumerService> logger, IConfiguration configuration, IMessageHandleService messageMediator)
    {
        _logger = logger;
        _configuration = configuration;
        _messageMediator = messageMediator;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _configuration["Kafka:BootstrapServers"],
            GroupId = _configuration["Kafka:GroupId"],
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        try
        {
            using (var consumer = new ConsumerBuilder<Ignore, string>(config).Build())
            {
                try
                {
                    var topicName = _configuration["Kafka:TopicName"];
                    consumer.Subscribe(topicName);
                    _logger.LogInformation($"Subscribed to topic: {topicName}");
                    while (!stoppingToken.IsCancellationRequested)
                    {
                        var consumeResult = consumer.Consume(stoppingToken);

                        try
                        {
                            var applicationSubmittedMessage = JsonSerializer.Deserialize<ApplicationSubmittedMessage>(consumeResult.Message.Value);
                            await _messageMediator.HandleMessage(applicationSubmittedMessage);
                        }
                        catch (JsonException ex)
                        {
                            _logger.LogError($"Error deserializing message: {ex.Message}");
                        }
                    }
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError($"Consume error: {ex.Error.Reason}");
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Consumer stopped.");
                }
                finally
                {
                    consumer.Close();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Consumer initialization error: {ex.Message}");
        }
    }
}