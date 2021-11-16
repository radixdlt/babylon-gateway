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
        var existingSubstate = await substates.SingleOrDefaultAsync(s => s.SubstateIdentifier == identifier, cancellationToken);
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
        Func<TSubstate, bool> verifySubstateMatches,
        LedgerOperationGroup downOperationGroup,
        int downOperationIndexInGroup,
        CancellationToken cancellationToken
    )
        where TSubstate : SubstateBase
    {
        var substate = await substates.SingleOrDefaultAsync(s => s.SubstateIdentifier == identifier, cancellationToken);
        if (substate == null)
        {
            // TODO - Handle down of virtual substates which didn't previously exist
            return;

            // throw new InvalidTransactionException(
            //     transactionOpLocator,
            //     $"{typeof(TSubstate).FullName} with identifier {identifier.ToHex()} could not be downed as it did not exist in the database"
            // );
        }

        if (substate.State == SubstateState.Down)
        {
            throw new InvalidTransactionException(
                transactionOpLocator,
                $"{typeof(TSubstate).FullName} with identifier {identifier.ToHex()} could not be downed as it was already down"
            );
        }

        if (!verifySubstateMatches(substate))
        {
            throw new InvalidTransactionException(
                transactionOpLocator,
                $"{typeof(TSubstate).FullName} with identifier {identifier.ToHex()} was downed, but the substate contents appear not to match at downing time"
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
        var existingHistoryItem = await history
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
