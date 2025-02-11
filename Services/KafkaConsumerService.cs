using System.Text;
using System.Text.Json;
using Confluent.Kafka;
using DataCollectionService.Messages;

namespace DataCollectionService;

public class KafkaConsumerService: BackgroundService
{
    private readonly ILogger<KafkaConsumerService> _logger;
    private readonly IConfiguration _configuration;
    
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _topicName;
    private readonly string _groupId;

    public KafkaConsumerService(ILogger<KafkaConsumerService> logger, IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _topicName = _configuration["Kafka:TopicName"];
        _groupId = _configuration["Kafka:GroupId"];
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _configuration["Kafka:BootstrapServers"],
            GroupId = _groupId,
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        try
        {
            using (var consumer = new ConsumerBuilder<Ignore, string>(config).Build())
            {
                try
                {
                    consumer.Subscribe(_topicName);
                    _logger.LogInformation($"Subscribed to topic: {_topicName}");
                    while (!stoppingToken.IsCancellationRequested)
                    {
                        var consumeResult = consumer.Consume(stoppingToken);

                        // Deserialize the message
                        try
                        {
                            var applicationSubmittedMessage = JsonSerializer.Deserialize<ApplicationSubmittedMessage>(consumeResult.Message.Value);

                            // Process the message and call ApiGateway
                            await ProcessMessage(applicationSubmittedMessage);
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
    private async Task ProcessMessage(ApplicationSubmittedMessage message)
    {
        // Construct the request to ApiGateway
        var client = _httpClientFactory.CreateClient();

        // API Gateway URL from configuration
        var apiGatewayUrl = _configuration["ApiGateway:BaseUrl"];
        var endpoint1 = _configuration["ApiGateway:Endpoint1"];
        var endpoint2 = _configuration["ApiGateway:Endpoint2"];

        // Serialize the message as JSON (or other format expected by ApiGateway)
        var jsonContent = JsonSerializer.Serialize(message);
       // var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        // Send POST requests to different endpoints in ApiGateway
        try
        {
            // Call endpoint 1
            var response1 = await client.PostAsJsonAsync($"{apiGatewayUrl}{endpoint1}", message);
            if (response1.IsSuccessStatusCode)
            {
                _logger.LogInformation($"ApiGateway call to {endpoint1} successful. Status code: {response1.StatusCode}");
            }
            else
            {
                _logger.LogError($"ApiGateway call to {endpoint1} failed. Status code: {response1.StatusCode}, Response: {await response1.Content.ReadAsStringAsync()}");
            }

            // Call endpoint 2
            var response2 = await client.PostAsJsonAsync($"{apiGatewayUrl}{endpoint2}", message);
            if (response2.IsSuccessStatusCode)
            {
                _logger.LogInformation($"ApiGateway call to {endpoint2} successful. Status code: {response2.StatusCode}");
            }
            else
            {
                _logger.LogError($"ApiGateway call to {endpoint2} failed. Status code: {response2.StatusCode}, Response: {await response2.Content.ReadAsStringAsync()}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error calling ApiGateway: {ex.Message}");
        }
    }
}