using System.Text.Json;
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
            var endpoint3 = _configuration["ApiGateway:Endpoint3"];
            
            var response1 = await CallApiAndHandleError(() => _apiGatewayService.CallApiGateway(message, endpoint1), "endpoint1","NBKICollectionName");
            var response2 = await CallApiAndHandleError(() => _apiGatewayService.CallApiGateway(message, endpoint2), "endpoint2","FNSCollectionName");
            var response3 = await CallApiAndHandleError(() => _apiGatewayService.CallApiGateway(message, endpoint3), "endpoint3","FSSPCollectionName");
            
            var dataCollectedMessage = new DataCollectedMessage(response1, response2, response3,
                new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString());
                
            await _kafkaProducerService.ProduceAsync("DataCollected",JsonSerializer.Serialize(dataCollectedMessage));
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error handling message: {ex.Message}");
        }
    }

    private async Task<string> CallApiAndHandleError(Func<Task<string>> apiCall, string endpointName, string collectionName)
    {
        try
        {
            var response =  await apiCall();
            if (!string.IsNullOrEmpty(response))
            {
                try
                {
                    await _databaseService.SaveCollectedData(collectionName, response);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error saving data to {collectionName}: {ex.Message}");
                }
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error calling ApiGateway for {endpointName}: {ex.Message}");
            return "Error calling ApiGateway";
        }
    }


}