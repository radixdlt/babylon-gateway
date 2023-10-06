using System.Net;

namespace RadixDlt.NetworkGateway.GatewayApi.Exceptions;

public class GenericBadRequestException : KnownGatewayErrorException
{
    public GenericBadRequestException(string userFacingMessage, string internalMessage)
        : base((int)HttpStatusCode.BadRequest, null, userFacingMessage, internalMessage)
    {
    }
}
