using Common.Extensions;
using Common.StaticHelpers;
using DataAggregator.Exceptions;
using RadixCoreApi.GeneratedClient.Model;

namespace DataAggregator.LedgerExtension;

public static class TransactionConsistency
{
    public static void AssertEqualParentIdentifiers(StateIdentifier parentStateIdentifierFromApi, TransactionSummary parentTransactionOverviewFromDb)
    {
        if (parentStateIdentifierFromApi.StateVersion != parentTransactionOverviewFromDb.StateVersion)
        {
            throw new InvalidLedgerCommitException(
                $"Attempted to commit a group of transactions with parent state version {parentStateIdentifierFromApi.StateVersion}," +
                $" but the last committed transaction is at stateVersion {parentTransactionOverviewFromDb.StateVersion}."
            );
        }

        var parentTransactionAccumulator = parentStateIdentifierFromApi.TransactionAccumulator.ConvertFromHex();
        if (!parentTransactionAccumulator.BytesAreEqual(parentTransactionOverviewFromDb.TransactionAccumulator))
        {
            throw new InconsistentLedgerException(
                $"Attempted to commit a group of transactions with parent transaction accumulator {parentTransactionAccumulator.ToHex()}," +
                $" (state version {parentStateIdentifierFromApi.StateVersion}) - but the last committed transaction" +
                $" in our database had accumulator {parentTransactionOverviewFromDb.TransactionAccumulator.ToHex()}"
            );
        }
    }

    public static void AssertChildTransactionConsistent(TransactionSummary parent, TransactionSummary child)
    {
        if (child.StateVersion != parent.StateVersion + 1)
        {
            throw new InvalidLedgerCommitException(
                $"Attempted to commit a transaction with state version {child.StateVersion}" +
                $" on top of transaction with state version {parent.StateVersion}"
            );
        }

        if (!AccumulatorVerifier.IsValidAccumulator(
                parent.TransactionAccumulator,
                child.TransactionIdentifierHash,
                child.TransactionAccumulator
            ))
        {
            throw new InconsistentLedgerException(
                $"Failure to commit a child transaction with resultant state version {child.StateVersion}." +
                $" The parent (with resultant state version {parent.StateVersion}) has accumulator {parent.TransactionAccumulator.ToHex()}" +
                $" and the child has transaction id hash {child.TransactionIdentifierHash.ToHex()}" +
                " which should result in an accumulator of" +
                $" {AccumulatorVerifier.CreateNewAccumulator(parent.TransactionAccumulator, child.TransactionIdentifierHash).ToHex()}" +
                $" but the child reports an inconsistent accumulator of {child.TransactionAccumulator.ToHex()}."
            );
        }
    }
}
