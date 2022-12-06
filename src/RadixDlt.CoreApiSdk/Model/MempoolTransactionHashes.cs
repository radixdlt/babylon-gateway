using Newtonsoft.Json;
using System;

namespace RadixDlt.CoreApiSdk.Model;

public partial class MempoolTransactionHashes
{
    private byte[] _payloadHashBytes;

    [JsonIgnore]
    public byte[] PayloadHashBytes => _payloadHashBytes ??= Convert.FromHexString(PayloadHash);
}
