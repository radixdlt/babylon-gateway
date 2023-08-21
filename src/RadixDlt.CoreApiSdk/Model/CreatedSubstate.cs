using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace RadixDlt.CoreApiSdk.Model;

public partial class StateUpdates
{
    public List<IAbc> MyCreated => CreatedSubstates.ToList<IAbc>();
    public List<IAbc> MyUpdate => UpdatedSubstates.ToList<IAbc>();
}

public partial class CreatedSubstate : IAbc
{
    public SubstateValue PreviousValue => null;
}

public partial class UpdatedSubstate : IAbc
{
    public SubstateValue Value => NewValue;
}

public interface IAbc
{
    public SubstateId SubstateId { get; }

    public SubstateValue Value { get; }

    public SubstateValue PreviousValue { get; }

    [MemberNotNullWhen(true, nameof(Value))]
    public bool HasValue => Value != null;

    [MemberNotNullWhen(true, nameof(PreviousValue))]
    public bool HasPreviousValue => PreviousValue != null;
}
