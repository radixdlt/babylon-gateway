namespace RadixDlt.NetworkGateway.GatewayApi.Exceptions;

public sealed class RequestTimeoutException : KnownGatewayErrorException
{
    /// <summary>
    /// Non standard status code (there's no suitable one for such situation).
    /// In cloudflare means that the server received complete request but it took to long to process on server side.
    /// Wanted to filter these cases out from standard internal server errors.
    /// </summary>
    private const int RequestTimeoutHttpStatusCode = 524;

    public RequestTimeoutException(string traceId, string internalMessage)
        : base(RequestTimeoutHttpStatusCode, null, $"Request timed out. If reporting this issue, please include TraceId={traceId}", internalMessage)
    {
    }
}
