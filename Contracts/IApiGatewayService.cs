using DataCollectionService.Messages;

namespace DataCollectionService.Contracts;

public interface IApiGatewayService
{
    Task<string> CallApiGateway(ApplicationSubmittedMessage message, string endpoint);
}