using Newtonsoft.Json;
using System;

namespace RadixDlt.CoreApiSdk.Model;

public partial class CommittedTransaction
{
    private byte[] _accumulatorHashBytes;

    [JsonIgnore]
    public byte[] AccumulatorHashBytes => _accumulatorHashBytes ??= Convert.FromHexString(AccumulatorHash);
}
