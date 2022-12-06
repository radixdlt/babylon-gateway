using System;
using System.Runtime.Serialization;

namespace RadixDlt.NetworkGateway.GatewayApiSdk.Model;

public partial class TransactionStatusRequest
{
    private byte[] _intentHashBytes;

    [IgnoreDataMember]
    public byte[] IntentHashBytes => _intentHashBytes ??= Convert.FromHexString(IntentHashHex);
}
