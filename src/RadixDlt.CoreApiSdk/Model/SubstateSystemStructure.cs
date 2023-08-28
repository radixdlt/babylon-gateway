using System.Collections.Generic;

namespace RadixDlt.CoreApiSdk.Model;

public abstract partial class SubstateSystemStructure : IEntityAddressPointer
{
    public abstract IEnumerable<string> GetEntityAddresses();
}
