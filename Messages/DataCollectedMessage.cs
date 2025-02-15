namespace DataCollectionService.Messages;

public record DataCollectedMessage (string ApiGatewayResponse1, string ApiGatewayResponse2, string ApiGatewayResponse3, string timestamp);