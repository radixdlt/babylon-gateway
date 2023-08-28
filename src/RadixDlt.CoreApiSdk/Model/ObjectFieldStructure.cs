using System.Collections.Generic;

namespace RadixDlt.CoreApiSdk.Model;

public partial class ObjectFieldStructure
{
    public override IEnumerable<string> GetEntityAddresses()
    {
        return ValueSchema.GetEntityAddresses();
    }
}
