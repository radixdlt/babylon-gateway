using Newtonsoft.Json;
using System;

namespace RadixDlt.CoreApiSdk.Model;

public partial class TransactionIntent
{
    private byte[] _hashBytes;

    [JsonIgnore]
    public byte[] HashBytes => _hashBytes ??= Convert.FromHexString(Hash);
}
