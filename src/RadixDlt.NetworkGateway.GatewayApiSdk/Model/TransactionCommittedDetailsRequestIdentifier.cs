using System;
using System.Runtime.Serialization;

namespace RadixDlt.NetworkGateway.GatewayApiSdk.Model;

public partial class TransactionCommittedDetailsRequestIdentifier
{
    private byte[] _valueBytes;

    [IgnoreDataMember]
    public byte[] ValueBytes => _valueBytes ??= Convert.FromHexString(ValueHex);
}
