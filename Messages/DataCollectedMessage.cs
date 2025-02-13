namespace DataCollectionService.Messages;

public class DataCollectedMessage
{
    public string OriginalMessageId { get; set; }
    public string ApiGatewayResponse1 { get; set; }
    public string ApiGatewayResponse2 { get; set; }
}