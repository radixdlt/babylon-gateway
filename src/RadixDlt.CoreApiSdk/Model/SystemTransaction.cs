using System;

namespace RadixDlt.CoreApiSdk.Model;

public partial class SystemTransaction
{
    private byte[] _payloadBytes;

    public byte[] GetPayloadBytes() => _payloadBytes ??= Convert.FromHexString(PayloadHex);
}
