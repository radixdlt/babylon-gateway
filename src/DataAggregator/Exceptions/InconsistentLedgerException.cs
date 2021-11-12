namespace DataAggregator.Exceptions;

/// <summary>
/// An Exception thrown when we try to commit a transaction, but we detect an inconsistency.
/// This suggests an error with the Ledger state itself.
/// </summary>
public class InconsistentLedgerException : Exception
{
    public InconsistentLedgerException(string message)
        : base(message)
    {
    }
}
