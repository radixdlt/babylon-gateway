namespace DataAggregator.Exceptions;

public record struct TransactionOpLocator(
    string TransactionIdHash,
    int? OperationGroupIndex,
    int? OperationIndexInGroup
);

/// <summary>
/// An Exception thrown when we attempt to commit a transaction which DataAggregator doesn't understand.
/// </summary>
public class InvalidTransactionException : Exception
{
    public InvalidTransactionException(
        TransactionOpLocator transactionOpLocator,
        string message
    )
        : this(transactionOpLocator.TransactionIdHash, transactionOpLocator.OperationGroupIndex, transactionOpLocator.OperationIndexInGroup, message)
    {
    }

    private InvalidTransactionException(
        string transactionIdHash,
        int? operationGroupIndex,
        int? operationIndexInGroup,
        string message
    )
        : base($"[TransactionId={transactionIdHash}, OpGroupIndex={operationGroupIndex}, OpIndex={operationIndexInGroup}] {message}")
    {
    }
}
