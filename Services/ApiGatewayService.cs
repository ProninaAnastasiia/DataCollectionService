using DataCollectionService.Contracts;
using DataCollectionService.Messages;

namespace DataCollectionService;

public class ApiGatewayService: IApiGatewayService
{
    private readonly ILogger<ApiGatewayService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public ApiGatewayService(ILogger<ApiGatewayService> logger, IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<bool> CallApiGateway(ApplicationSubmittedMessage message, string endpoint)
    {
        var client = _httpClientFactory.CreateClient();
        var apiGatewayUrl = _configuration["ApiGateway:BaseUrl"];

        try
        {
            var response = await client.PostAsJsonAsync($"{apiGatewayUrl}{endpoint}", message);
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation($"ApiGateway call to {endpoint} successful. Status code: {response.StatusCode}");
                return true;
            }
            else
            {
                _logger.LogError($"ApiGateway call to {endpoint} failed. Status code: {response.StatusCode}, Response: {await response.Content.ReadAsStringAsync()}");
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error calling ApiGateway: {ex.Message}");
            return false;
        }
    }
}