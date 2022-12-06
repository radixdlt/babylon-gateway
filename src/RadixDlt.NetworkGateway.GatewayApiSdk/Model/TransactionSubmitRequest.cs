using System;
using System.Runtime.Serialization;

namespace RadixDlt.NetworkGateway.GatewayApiSdk.Model;

public partial class TransactionSubmitRequest
{
    private byte[] _notarizedTransactionBytes;

    [IgnoreDataMember]
    public byte[] NotarizedTransactionBytes => _notarizedTransactionBytes ??= Convert.FromHexString(NotarizedTransactionHex);
}
