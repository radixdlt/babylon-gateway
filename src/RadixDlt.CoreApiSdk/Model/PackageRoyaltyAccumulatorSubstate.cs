using System.Collections.Generic;

namespace RadixDlt.CoreApiSdk.Model;

public partial class PackageRoyaltyAccumulatorSubstate : IOwner
{
    public IEnumerable<EntityReference> GetOwnedEntities()
    {
        yield return VaultEntity;
    }
}
