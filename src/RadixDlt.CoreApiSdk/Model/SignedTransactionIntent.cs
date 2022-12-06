using Newtonsoft.Json;
using System;

namespace RadixDlt.CoreApiSdk.Model;

public partial class SignedTransactionIntent
{
    private byte[] _hashBytes;

    [JsonIgnore]
    public byte[] HashBytes => _hashBytes ??= Convert.FromHexString(Hash);
}
