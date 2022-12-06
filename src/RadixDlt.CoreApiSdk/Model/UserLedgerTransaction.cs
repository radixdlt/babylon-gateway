using Newtonsoft.Json;
using System;

namespace RadixDlt.CoreApiSdk.Model;

public partial class UserLedgerTransaction
{
    private byte[] _payloadBytes;

    [JsonIgnore]
    public byte[] PayloadBytes => _payloadBytes ??= Convert.FromHexString(PayloadHex);
}
