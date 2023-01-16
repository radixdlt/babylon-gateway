using System.Collections.Generic;

namespace RadixDlt.CoreApiSdk.Model;

public partial class ComponentRoyaltyAccumulatorSubstate : IOwner
{
    public IEnumerable<EntityReference> GetOwnedEntities()
    {
        yield return VaultEntity;
    }
}
