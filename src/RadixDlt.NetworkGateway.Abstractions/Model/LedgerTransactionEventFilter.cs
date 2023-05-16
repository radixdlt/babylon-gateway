using RadixDlt.NetworkGateway.Abstractions.Numerics;

namespace RadixDlt.NetworkGateway.Abstractions.Model;

public class LedgerTransactionEventFilter
{
    public enum EventType
    {
        Withdrawal,
        Deposit,
    }

    public EventType Event { get; set; }

    public GlobalAddress? EmitterEntityAddress { get; set; }

    public GlobalAddress? ResourceAddress { get; set; }

    public TokenAmount? Qunatity { get; set; }
}
