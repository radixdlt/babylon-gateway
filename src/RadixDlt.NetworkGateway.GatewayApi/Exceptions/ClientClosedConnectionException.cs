namespace RadixDlt.NetworkGateway.GatewayApi.Exceptions;

public sealed class ClientClosedConnectionException : KnownGatewayErrorException
{
    public ClientClosedConnectionException(string userFacingMessage, string internalMessage)
        : base(499, null, userFacingMessage, internalMessage)
    {
    }
}
