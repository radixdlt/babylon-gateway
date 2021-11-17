using Common.Database;
using Common.Database.Models.Ledger;
using Common.Database.Models.Ledger.History;
using Common.Database.Models.Ledger.Substates;
using Common.Extensions;
using DataAggregator.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DataAggregator.Extensions;

public static class DbSetExtensions
{
    public static async Task UpSubstate<TSubstate>(
        this DbSet<TSubstate> substates,
        TransactionOpLocator transactionOpLocator,
        byte[] identifier,
        TSubstate newSubstate,
        LedgerOperationGroup upOperationGroup,
        int upOperationIndexInGroup,
        CancellationToken cancellationToken
    )
        where TSubstate : SubstateBase
    {
        // Could rely on the database to check this constraint at commit time, but this gives us a clearer error
        var existingSubstate = substates
                                   .Local
                                   .SingleOrDefault(s => s.SubstateIdentifier.BytesAreEqual(identifier))
                               ?? await substates
                                   .AsNoTracking()
                                   .SingleOrDefaultAsync(s => s.SubstateIdentifier == identifier, cancellationToken);
        if (existingSubstate != null)
        {
            throw new InvalidTransactionException(
                transactionOpLocator,
                $"{typeof(TSubstate).FullName} with identifier {identifier.ToHex()} can't be upped, as a substate with that identifier already already exists in the database"
            );
        }

        newSubstate.SubstateIdentifier = identifier;
        newSubstate.UpOperationGroup = upOperationGroup;
        newSubstate.UpOperationIndexInGroup = upOperationIndexInGroup;

        substates.Add(newSubstate);
    }

    public static async Task DownSubstate<TSubstate>(
        this DbSet<TSubstate> substates,
        TransactionOpLocator transactionOpLocator,
        byte[] identifier,
        Func<TSubstate> createNewSubstateIfVirtual,
        Func<TSubstate, bool> verifySubstateMatches,
        LedgerOperationGroup downOperationGroup,
        int downOperationIndexInGroup,
        CancellationToken cancellationToken
    )
        where TSubstate : SubstateBase
    {
        var substate = substates
                           .Local
                           .SingleOrDefault(s => s.SubstateIdentifier.BytesAreEqual(identifier))
                       ?? await substates
                           .SingleOrDefaultAsync(s => s.SubstateIdentifier == identifier, cancellationToken);
        if (substate == null)
        {
            if (!SubstateBase.IsVirtualIdentifier(identifier))
            {
                throw new InvalidTransactionException(
                    transactionOpLocator,
                    $"Non-virtual {typeof(TSubstate).Name} with identifier {identifier.ToHex()} could not be downed as it did not exist in the database"
                );
            }

            // Virtual substates can be downed without being upped
            var newSubstate = createNewSubstateIfVirtual();
            newSubstate.SubstateIdentifier = identifier;
            newSubstate.UpOperationGroup = downOperationGroup;
            newSubstate.UpOperationIndexInGroup = downOperationIndexInGroup;
            newSubstate.DownOperationGroup = downOperationGroup;
            newSubstate.DownOperationIndexInGroup = downOperationIndexInGroup;
            substates.Add(newSubstate);
            return;
        }

        if (substate.State == SubstateState.Down)
        {
            throw new InvalidTransactionException(
                transactionOpLocator,
                $"{typeof(TSubstate).Name} with identifier {identifier.ToHex()} could not be downed as it was already down"
            );
        }

        if (!verifySubstateMatches(substate))
        {
            throw new InvalidTransactionException(
                transactionOpLocator,
                $"{typeof(TSubstate).Name} with identifier {identifier.ToHex()} was downed, but the substate contents appear not to match at downing time"
            );
        }

        substate.DownOperationGroup = downOperationGroup;
        substate.DownOperationIndexInGroup = downOperationIndexInGroup;
    }

    /// <summary>
    /// Creates a new history entry and closes out the previous one (if it exists).
    ///
    /// Note that:
    /// * historySelector does not need to care about the StateVersion.
    /// * createNewHistory does not need to care about the StateVersion.
    /// </summary>
    /// <typeparam name="THistory">The history entity type.</typeparam>
    public static async Task AddNewHistoryEntry<THistory>(
        this DbSet<THistory> history,
        Expression<Func<THistory, bool>> historySelector,
        Func<THistory?, THistory> createNewHistory,
        long transactionStateVersion,
        CancellationToken cancellationToken
    )
        where THistory : HistoryBase
    {
        var existingHistoryItem = history
            .Local
            .Where(historySelector.Compile())
            .FirstOrDefault(h => h.ToStateVersion == null)
            ?? await history
            .Where(historySelector)
            .FirstOrDefaultAsync(h => h.ToStateVersion == null, cancellationToken);

        if (existingHistoryItem == null)
        {
            var newHistoryItem = createNewHistory(null);
            history.Add(newHistoryItem);
        }
        else
        {
            var newHistoryItem = createNewHistory(existingHistoryItem);
            existingHistoryItem.ToStateVersion = transactionStateVersion;
            newHistoryItem.FromStateVersion = transactionStateVersion;
            history.Add(newHistoryItem);
        }
    }
}
