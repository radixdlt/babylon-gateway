using System.Collections.Generic;

namespace RadixDlt.CoreApiSdk.Model;

// TODO rename
// a) indicates that this model type is owner_ancestor_id
// b) has owned_entities
public interface IOwner
{
    public List<EntityId> OwnedEntities { get; }
}
