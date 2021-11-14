using Common.Database;
using Common.Database.Models;
using Common.Database.Models.Ledger;
using Common.Extensions;
using Common.Numerics;
using Common.StaticHelpers;
using DataAggregator.Exceptions;
using Microsoft.EntityFrameworkCore;
using RadixCoreApi.GeneratedClient.Model;

namespace DataAggregator.GlobalServices;

public interface ITransactionCommitter
{
    Task CommitTransactions(StateIdentifier parentStateIdentifier, List<CommittedTransaction> committedTransactions, CancellationToken token);

    Task<long> GetTopOfLedgerStateVersion(CancellationToken token);
}

public class TransactionCommitter : ITransactionCommitter
{
    private readonly IDbContextFactory<CommonDbContext> _contextFactory;
    private readonly IRawTransactionWriter _rawTransactionWriter;

    public TransactionCommitter(
        IDbContextFactory<CommonDbContext> contextFactory,
        IRawTransactionWriter rawTransactionWriter)
    {
        _contextFactory = contextFactory;
        _rawTransactionWriter = rawTransactionWriter;
    }

    public async Task<long> GetTopOfLedgerStateVersion(CancellationToken token)
    {
        await using var dbContext = await _contextFactory.CreateDbContextAsync(token);
        var lastTransactionOverview = await GetLastCommittedTransactionOverview(dbContext, token);
        return lastTransactionOverview.StateVersion;
    }

    public async Task CommitTransactions(StateIdentifier parentStateIdentifier, List<CommittedTransaction> transactionsIn, CancellationToken token)
    {
        // Create own context for this transaction
        await using var dbContext = await _contextFactory.CreateDbContextAsync(token);
        var lastTransactionOverview = await GetLastCommittedTransactionOverview(dbContext, token);

        var transactionsToCommit = transactionsIn
            .Where(t => t.CommittedStateIdentifier.StateVersion >= lastTransactionOverview.StateVersion)
            .ToList();

        var rawTransactions = transactionsToCommit.Select(CreateRawTransaction).ToList();
        await _rawTransactionWriter.EnsureRawTransactionsCreatedOrUpdated(dbContext, rawTransactions, token);

        AssertParentTransactionsAgree(parentStateIdentifier, lastTransactionOverview);

        foreach (var transaction in transactionsToCommit)
        {
            var transactionOverview = CreateTransactionOverview(lastTransactionOverview, transaction);
            AssertNextTransactionConsistent(lastTransactionOverview, transactionOverview);
            var ledgerTransaction = CreateLedgerTransactionShell(transaction, transactionOverview);
            await dbContext.LedgerTransactions.AddAsync(ledgerTransaction, token);
            lastTransactionOverview = transactionOverview;
        }

        await dbContext.SaveChangesAsync(token);
    }

    private void AssertParentTransactionsAgree(StateIdentifier parentStateIdentifierFromApi, TransactionOverview parentTransactionOverviewFromDb)
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

    private void AssertNextTransactionConsistent(TransactionOverview parent, TransactionOverview child)
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

    public record TransactionOverview(long StateVersion, long Epoch, long IndexInEpoch, bool IsEndOfEpoch, byte[] TransactionIdentifierHash, byte[] TransactionAccumulator, long? EndOfEpochRound);

    private static async Task<TransactionOverview> GetLastCommittedTransactionOverview(CommonDbContext dbContext, CancellationToken token)
    {
        var lastOverview = await dbContext.LedgerTransactions
            .AsNoTracking()
            .OrderByDescending(lt => lt.ResultantStateVersion)
            .Select(lt => new TransactionOverview(lt.ResultantStateVersion, lt.Epoch, lt.IndexInEpoch, lt.IsEndOfEpoch, lt.TransactionIdentifierHash, lt.TransactionAccumulator, lt.EndOfEpochRound))
            .FirstOrDefaultAsync(token);

        return lastOverview ?? GetPreGenesisTransactionOverview();
    }

    private static TransactionOverview GetPreGenesisTransactionOverview()
    {
        return new TransactionOverview(
            StateVersion: 0,
            Epoch: 0,
            IndexInEpoch: -1,
            IsEndOfEpoch: false,
            TransactionAccumulator: Convert.FromHexString("0000000000000000000000000000000000000000000000000000000000000000"),
            TransactionIdentifierHash: Array.Empty<byte>(),
            EndOfEpochRound: null
        );
    }

    private static TransactionOverview CreateTransactionOverview(TransactionOverview lastTransaction, CommittedTransaction transaction)
    {
        long? newEpochForNextTransaction = null;
        long? endOfEpochRound = null;
        foreach (var operationGroup in transaction.OperationGroups)
        {
            foreach (var operation in operationGroup.Operations)
            {
                if (operation.Data?.Action == Data.ActionEnum.CREATE &&
                    operation.Data.DataObject is EpochData epochData)
                {
                    newEpochForNextTransaction = epochData.Epoch;
                }

                if (operation.Data?.Action == Data.ActionEnum.DELETE &&
                    operation.Data.DataObject is RoundData roundData)
                {
                    endOfEpochRound = roundData.Round;
                }
            }
        }

        return new TransactionOverview(
            StateVersion: transaction.CommittedStateIdentifier.StateVersion,
            Epoch: lastTransaction.IsEndOfEpoch ? lastTransaction.Epoch + 1 : lastTransaction.Epoch,
            IndexInEpoch: lastTransaction.IsEndOfEpoch ? 0 : lastTransaction.IndexInEpoch + 1,
            IsEndOfEpoch: newEpochForNextTransaction != null,
            TransactionIdentifierHash: transaction.TransactionIdentifier.Hash.ConvertFromHex(),
            TransactionAccumulator: transaction.CommittedStateIdentifier.TransactionAccumulator.ConvertFromHex(),
            EndOfEpochRound: endOfEpochRound
        );
    }

    private static RawTransaction CreateRawTransaction(CommittedTransaction transaction)
    {
        return new RawTransaction(
            transactionIdentifierHash: transaction.TransactionIdentifier.Hash.ConvertFromHex(),
            submittedTimestamp: null,
            payload: transaction.Metadata.Hex.ConvertFromHex()
        );
    }

    private static LedgerTransaction CreateLedgerTransactionShell(CommittedTransaction transaction, TransactionOverview overview)
    {
        var resultantStateVersion = overview.StateVersion;
        long? parentStateVersion = resultantStateVersion > 1 ? resultantStateVersion - 1 : null;
        return new LedgerTransaction(
            resultantStateVersion: resultantStateVersion,
            parentStateVersion: parentStateVersion,
            transactionIdentifierHash: overview.TransactionIdentifierHash,
            transactionAccumulator: overview.TransactionAccumulator,
            message: transaction.Metadata.Message?.ConvertFromHex(),
            feePaid: TokenAmount.FromString(transaction.Metadata.Fee.Value),
            epoch: overview.Epoch,
            indexInEpoch: overview.IndexInEpoch,
            isEndOfEpoch: overview.IsEndOfEpoch,
            timestamp: DateTimeOffset.FromUnixTimeMilliseconds(transaction.Metadata.Timestamp).UtcDateTime,
            endOfEpochRound: overview.EndOfEpochRound
        );
    }
}
