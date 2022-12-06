using Newtonsoft.Json;
using System;

namespace RadixDlt.CoreApiSdk.Model;

public partial class SborData
{
    private byte[] _dataBytes;

    [JsonIgnore]
    public byte[] DataBytes => _dataBytes ??= Convert.FromHexString(DataHex);
}
