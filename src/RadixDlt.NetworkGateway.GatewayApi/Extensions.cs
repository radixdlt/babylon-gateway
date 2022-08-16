using RadixDlt.NetworkGateway.Common;
using RadixDlt.NetworkGateway.GatewayApi.Services;
using System;

namespace RadixDlt.NetworkGateway.GatewayApi;

internal static class Extensions
{
    public static ValidatedTransactionIdentifier ToTransactionIdentifier(this string input)
    {
        var bytes = Convert.FromHexString(input);

        if (bytes.Length != NetworkGatewayConstants.Transaction.IdentifierByteLength)
        {
            throw new ArgumentException($"Expected HEX representation of {NetworkGatewayConstants.Transaction.IdentifierByteLength} bytes, {bytes.Length} given.", nameof(input));
        }

        return new ValidatedTransactionIdentifier(input, bytes);
    }
}
