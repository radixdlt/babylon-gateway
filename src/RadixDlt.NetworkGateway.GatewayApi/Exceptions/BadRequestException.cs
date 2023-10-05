namespace RadixDlt.NetworkGateway.GatewayApi.Exceptions;

public sealed class BadRequestException : KnownGatewayErrorException
{
    public BadRequestException(string userFacingMessage, string internalMessage)
        : base(400, null, userFacingMessage, internalMessage)
    {
    }
}
