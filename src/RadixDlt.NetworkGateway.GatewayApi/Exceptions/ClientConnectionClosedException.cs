namespace RadixDlt.NetworkGateway.GatewayApi.Exceptions;

public sealed class ClientConnectionClosedException : KnownGatewayErrorException
{
    /// <summary>
    /// Non standard status code (there's no suitable one for such situation).
    /// In NGINX means that the client closed the connection before the server answered the request.
    /// Wanted to filter these cases out from standard internal server errors.
    /// </summary>
    private const int ClientConnectionClosedHttpStatusCode = 499;

    public ClientConnectionClosedException(string userFacingMessage, string internalMessage)
        : base(ClientConnectionClosedHttpStatusCode, null, userFacingMessage, internalMessage)
    {
    }
}
