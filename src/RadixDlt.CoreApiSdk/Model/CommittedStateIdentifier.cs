using Newtonsoft.Json;
using System;

namespace RadixDlt.CoreApiSdk.Model;

public partial class CommittedStateIdentifier
{
    private byte[] _accumulatorHashBytes;

    [JsonIgnore]
    public byte[] AccumulatorHashBytes => _accumulatorHashBytes ??= Convert.FromHexString(AccumulatorHash);
}
