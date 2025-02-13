using DataCollectionService.Messages;

namespace DataCollectionService.Contracts;

public interface IApiGatewayService
{
    Task<bool> CallApiGateway(ApplicationSubmittedMessage message, string endpoint);
}