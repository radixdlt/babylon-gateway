using Newtonsoft.Json;
using System;

namespace RadixDlt.CoreApiSdk.Model;

public partial class PackageInfoSubstate
{
    private byte[] _codeBytes;

    [JsonIgnore]
    public byte[] CodeBytes => _codeBytes ??= Convert.FromHexString(CodeHex);
}
