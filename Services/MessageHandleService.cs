using DataCollectionService.Contracts;
using DataCollectionService.Messages;

namespace DataCollectionService;

public class MessageHandleService: IMessageHandleService
{
    private readonly ILogger<MessageHandleService> _logger;
    private readonly IApiGatewayService _apiGatewayService;
    private readonly IDatabaseService _databaseService;
    private readonly IKafkaProducerService _kafkaProducerService;
    private readonly IConfiguration _configuration;

    public MessageHandleService(ILogger<MessageHandleService> logger, IApiGatewayService apiGatewayService, IDatabaseService databaseService, IKafkaProducerService kafkaProducerService, IConfiguration configuration)
    {
        _logger = logger;
        _apiGatewayService = apiGatewayService;
        _databaseService = databaseService;
        _kafkaProducerService = kafkaProducerService;
        _configuration = configuration;
    }

    public async Task HandleMessage(ApplicationSubmittedMessage message)
    {
        try
        {
            var endpoint1 = _configuration["ApiGateway:Endpoint1"];
            var endpoint2 = _configuration["ApiGateway:Endpoint2"];
            
            var response1Success = await _apiGatewayService.CallApiGateway(message, endpoint1);
            var response2Success = await _apiGatewayService.CallApiGateway(message, endpoint2);

            // Save response to database (example)
            if (response1Success && response2Success)
            {
                
            } else
            {
                _logger.LogError("At least one api call failed");
            }
            
            // Publish DataCollectedMessage
            var dataCollectedMessage = new DataCollectedMessage { /* Populate with data from message and responses */ };
            await _kafkaProducerService.PublishDataCollectedMessage(dataCollectedMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error handling message: {ex.Message}");
        }
    }
}