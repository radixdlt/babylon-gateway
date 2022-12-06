using Newtonsoft.Json;
using System;

namespace RadixDlt.CoreApiSdk.Model;

public partial class ValidatorLedgerTransaction
{
    private byte[] _payloadBytes;

    [JsonIgnore]
    public byte[] PayloadBytes => _payloadBytes ??= Convert.FromHexString(PayloadHex);
}
