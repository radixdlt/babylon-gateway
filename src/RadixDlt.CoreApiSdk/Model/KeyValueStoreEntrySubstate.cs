using System.Collections.Generic;

namespace RadixDlt.CoreApiSdk.Model;

public partial class KeyValueStoreEntrySubstate : IOwner
{
    public List<EntityId> OwnedEntities => DataStruct.OwnedEntities;
}
