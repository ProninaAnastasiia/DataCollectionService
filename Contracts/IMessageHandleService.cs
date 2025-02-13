using DataCollectionService.Messages;

namespace DataCollectionService.Contracts;

public interface IMessageHandleService
{
    Task HandleMessage(ApplicationSubmittedMessage message);
}