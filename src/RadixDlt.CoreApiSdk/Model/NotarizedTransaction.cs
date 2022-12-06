using Newtonsoft.Json;
using System;

namespace RadixDlt.CoreApiSdk.Model;

public partial class NotarizedTransaction
{
    private byte[] _payloadBytes;
    private byte[] _hashBytes;

    [JsonIgnore]
    public byte[] PayloadBytes => _payloadBytes ??= Convert.FromHexString(PayloadHex);

    [JsonIgnore]
    public byte[] HashBytes => _hashBytes ??= Convert.FromHexString(Hash);
}
