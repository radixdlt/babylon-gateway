namespace DataAggregator.Exceptions;

/// <summary>
/// An Exception thrown when we attempt to commit a transaction which is inconsistent with the transactions before it.
/// This suggests an error with the DataAggregator.
/// </summary>
public class InvalidLedgerCommitException : Exception
{
    public InvalidLedgerCommitException(string message)
        : base(message)
    {
    }
}
