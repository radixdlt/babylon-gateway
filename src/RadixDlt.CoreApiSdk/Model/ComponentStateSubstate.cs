using System.Collections.Generic;

namespace RadixDlt.CoreApiSdk.Model;

public partial class ComponentStateSubstate : IOwner
{
    public List<EntityId> OwnedEntities => DataStruct.OwnedEntities;
}
