using System.Collections.Generic;
using System.Linq;

namespace RadixDlt.CoreApiSdk.Model;

public partial class ObjectSortedIndexPartitionEntryStructure
{
    public override IEnumerable<string> GetEntityAddresses()
    {
        return KeySchema.GetEntityAddresses().Concat(ValueSchema.GetEntityAddresses());
    }
}
