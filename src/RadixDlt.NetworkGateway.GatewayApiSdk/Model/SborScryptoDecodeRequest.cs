using System;

namespace RadixDlt.NetworkGateway.GatewayApiSdk.Model;

public partial class SborScryptoDecodeRequest
{
    private byte[] _valueBytes;

    public byte[] GetValueBytes() => _valueBytes ??= Convert.FromHexString(ValueHex);
}
