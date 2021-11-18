using Common.Database;
using Common.Database.Models.Ledger;
using Common.Database.Models.Ledger.History;
using Common.Database.Models.Ledger.Substates;
using Common.Extensions;
using Common.Numerics;
using DataAggregator.Exceptions;
using DataAggregator.Extensions;
using DataAggregator.GlobalServices;
using Microsoft.EntityFrameworkCore;
using RadixCoreApi.GeneratedClient.Model;
using System.Linq.Expressions;

namespace DataAggregator.LedgerExtension;

/// <summary>
/// A short-lived stateful class for extracting the content of a transaction.
/// </summary>
public class TransactionContentCommitter
{
    /* Dependencies */
    private readonly CommonDbContext _dbContext;
    private readonly IEntityDeterminer _entityDeterminer;
    private readonly CancellationToken _cancellationToken;

    /* History */
    private readonly Dictionary<AccountResource, TokenAmount> _accountResourceNetBalanceChanges = new();

    /* Mutable Class State */
    /* > These simply help us avoid passing tons of references down the call stack.
    /* > These will all not be null at the time of use in the Handle methods. */
    private TransactionSummary? _transactionSummary;
    private CommittedTransaction? _transaction;
    private LedgerOperationGroup? _dbOperationGroup;
    private OperationGroup? _transactionOperationGroup;
    private int? _operationGroupIndex;
    private Operation? _operation;
    private int? _operationIndexInGroup;
    private Entity? _entity;
    private TokenAmount? _amount;

    public TransactionContentCommitter(CommonDbContext dbContext, IEntityDeterminer entityDeterminer, CancellationToken cancellationToken)
    {
        _dbContext = dbContext;
        _entityDeterminer = entityDeterminer;
        _cancellationToken = cancellationToken;
    }

    // TODO:NG-49 - Consider parallelising by using DBContextFactory instead of awaiting each separate update(!)
    // BUT:
    //  * We'll still want to tie them all into the same transaction, but will likely need to run SaveChanges in series
    //    as I doubt SaveChangesAsync is threadsafe on a transaction
    //  * We'll need any local history lookups to work across all different db contexts (or ensure they end up with the
    //    same dbcontext, by, eg, having a map from Substate Identifier / History Type and Key to used db context, so
    //    that we can streamline any such updates across that context)
    //  * We could also consider batching look-ups against a given table to speed these requests up, or even just
    //    make the thing lazy, and do the batched-lookups, updates and writes all at the end of a transaction batch.
    public async Task CommitTransactionDetails(CommittedTransaction transaction, TransactionSummary transactionSummary)
    {
        _transaction = transaction;
        _transactionSummary = transactionSummary;
        foreach (var (operationGroup, operationGroupIndex) in transaction.SubstantiveOperationGroups())
        {
            _transactionOperationGroup = operationGroup;
            _operationGroupIndex = operationGroupIndex;
            _dbOperationGroup = new LedgerOperationGroup(
                transaction.CommittedStateIdentifier.StateVersion,
                operationGroupIndex,
                null // TODO:NG-41 - fix inferred actions
            );
            _dbContext.OperationGroups.Add(_dbOperationGroup);

            foreach (var (operation, operationIndexInGroup) in _transactionOperationGroup.SubstantiveOperations())
            {
                _operation = operation;
                _operationIndexInGroup = operationIndexInGroup;
                _entity = _entityDeterminer.DetermineEntity(_operation.EntityIdentifier);
                if (_entity == null)
                {
                    throw GenerateDetailedInvalidTransactionException(
                        $"Failed to identify the entity"
                    );
                }

                await HandleOperation();
            }
        }

        await HandleHistoryUpdates();
    }

    private async Task HandleOperation()
    {
        if (_operation!.Amount != null)
        {
            await HandleAmountOperation();
        }

        if (_operation!.Data != null)
        {
            await HandleDataOperation();
        }
    }

    private Task HandleAmountOperation()
    {
        _amount = TokenAmount.FromSubUnitsString(_operation!.Amount.Value);
        if (_amount.Value.IsNaN())
        {
            throw GenerateDetailedInvalidTransactionException(
                $"Unparsable token amount value: {_operation!.Amount.Value}"
            );
        }

        switch (_operation!.Amount.ResourceIdentifier)
        {
            case TokenResourceIdentifier resourceIdentifier:
                return HandleResourceAmountOperation(resourceIdentifier);
            case StakeOwnershipResourceIdentifier stakeOwnershipResourceIdentifier:
                return HandleStakeOwnershipAmountOperation(stakeOwnershipResourceIdentifier);
            default:
                throw GenerateDetailedInvalidTransactionException(
                    $"Unknown resource identifier type: {_operation!.Amount.ResourceIdentifier.Type}"
                );
        }
    }

    private Task HandleResourceAmountOperation(TokenResourceIdentifier resourceIdentifier)
    {
        switch (_entity!.EntityType)
        {
            case EntityType.Account:
                return HandleAccountResourceAmountOperation(_entity!.AccountAddress!, resourceIdentifier.Rri);
            case EntityType.Account_ExitingStake:
            case EntityType.Account_PreparedStake:
                return HandleAccountXrdStakeResourceAmountOperation(_entity!, resourceIdentifier.Rri);
            case EntityType.Validator_System:
                return HandleValidatorResourceAmountOperation(_entity!.ValidatorAddress!, resourceIdentifier.Rri);
            case EntityType.Account_PreparedUnstake:
            case EntityType.System:
            case EntityType.Validator:
            case EntityType.Resource:
            default:
                throw GenerateDetailedInvalidTransactionException(
                    $"Resource Amount operation against unsupported entity type: {_entity!.EntityType}"
                );
        }
    }

    private Task HandleStakeOwnershipAmountOperation(StakeOwnershipResourceIdentifier stakeOwnershipResourceIdentifier)
    {
        switch (_entity!.EntityType)
        {
            case EntityType.Account:
            case EntityType.Account_PreparedUnstake:
                return HandleAccountStakeOwnershipAmountOperation(_entity!.EntityType, _entity!.AccountAddress!, stakeOwnershipResourceIdentifier.Validator);
            case EntityType.Account_ExitingStake:
            case EntityType.Account_PreparedStake:
            case EntityType.Validator_System:
            case EntityType.System:
            case EntityType.Validator:
            case EntityType.Resource:
            default:
                throw GenerateDetailedInvalidTransactionException(
                    $"Stake ownership amount operation against unsupported entity type: {_entity!.EntityType}"
                );
        }
    }

    private async Task HandleAccountResourceAmountOperation(string accountAddress, string resourceIdentifier)
    {
        var accountResource = new AccountResource(accountAddress, resourceIdentifier);

        // Part 1) Handle substates
        await HandleSubstateUpOrDown(
            _dbContext.AccountResourceBalanceSubstates,
            () => new AccountResourceBalanceSubstate(accountResource, _amount!.Value),
            existingSubstate => (
                existingSubstate.AccountAddress == accountAddress
                && existingSubstate.ResourceIdentifier == resourceIdentifier
                && existingSubstate.Amount == -_amount!.Value // Negative because downed has the opposite amount as upped
            )
        );

        // Part 2) Handle history
        _accountResourceNetBalanceChanges.TrackBalanceDelta(accountResource, TokenAmount.FromSubUnitsString(_operation!.Amount.Value));
    }

    private async Task HandleAccountXrdStakeResourceAmountOperation(Entity entity, string resourceIdentifier)
    {
        if (!_entityDeterminer.IsXrd(resourceIdentifier))
        {
            throw GenerateDetailedInvalidTransactionException(
                $"Resource Amount operation against unsupported entity type: {_entity!.EntityType}"
            );
        }

        var type = entity.EntityType! switch
        {
            EntityType.Account_PreparedStake => AccountXrdStakeBalanceSubstateType.PreparedStake,
            EntityType.Account_ExitingStake => AccountXrdStakeBalanceSubstateType.ExitingStake,
            _ => throw new ArgumentOutOfRangeException(),
        };

        // Part 1) Handle substates
        await HandleSubstateUpOrDown(
            _dbContext.AccountXrdStakeBalanceSubstates,
            () => new AccountXrdStakeBalanceSubstate(
                entity.AccountAddress!,
                entity.ValidatorAddress!,
                type,
                entity.EpochUnlock,
                _amount!.Value
            ),
            existingSubstate => (
                existingSubstate.AccountAddress == entity.AccountAddress!
                && existingSubstate.ValidatorAddress == entity.ValidatorAddress!
                && existingSubstate.Type == type
                && existingSubstate.UnlockEpoch == entity.EpochUnlock
                && existingSubstate.Amount == -_amount!.Value // Negative because downed has the opposite amount as upped
            )
        );

        // No history in this case
    }

    private async Task HandleValidatorResourceAmountOperation(string validatorAddress, string resourceIdentifier)
    {
        if (!_entityDeterminer.IsXrd(resourceIdentifier))
        {
            throw GenerateDetailedInvalidTransactionException(
                $"Resource Amount operation against unsupported entity type: {_entity!.EntityType}"
            );
        }

        // Part 1) Handle substates
        await HandleSubstateUpOrDown(
            _dbContext.ValidatorStakeBalanceSubstates,
            () => new ValidatorStakeBalanceSubstate(
                validatorAddress,
                _transactionSummary!.Epoch, // Put in for history's sake, but not part of the substate, so shouldn't be verified against
                _amount!.Value
            ),
            existingSubstate => (
                existingSubstate.ValidatorAddress == validatorAddress
                && existingSubstate.Amount == -_amount!.Value // Negative because downed has the opposite amount as upped
            )
        );

        // No history in this case
    }

    private async Task HandleAccountStakeOwnershipAmountOperation(EntityType entityType, string accountAddress, string validatorAddress)
    {
        var type = entityType switch
        {
            EntityType.Account => AccountStakeOwnershipBalanceSubstateType.Staked,
            EntityType.Account_PreparedUnstake => AccountStakeOwnershipBalanceSubstateType.PreparingUnstake,
            _ => throw new ArgumentOutOfRangeException(),
        };

        // Part 1) Handle substates
        await HandleSubstateUpOrDown(
            _dbContext.AccountStakeOwnershipBalanceSubstates,
            () => new AccountStakeOwnershipBalanceSubstate(
                accountAddress,
                validatorAddress,
                type,
                _amount!.Value
            ),
            existingSubstate => (
                existingSubstate.AccountAddress == accountAddress
                && existingSubstate.ValidatorAddress == validatorAddress
                && existingSubstate.Type == type
                && existingSubstate.Amount == -_amount!.Value // Negative because downed has the opposite amount as upped
            )
        );

        // No history yet...
    }

    private Task HandleDataOperation()
    {
        // TODO:NG-24
        return Task.CompletedTask;
    }

    private async Task HandleHistoryUpdates()
    {
        await HandleAccountResourceBalanceHistoryUpdates();
    }

    private async Task HandleAccountResourceBalanceHistoryUpdates()
    {
        var accountResourceHistoryChanges = _accountResourceNetBalanceChanges
            .Where(arnb => arnb.Value != TokenAmount.Zero);

        foreach (var (key, entry) in accountResourceHistoryChanges)
        {
            if (entry.IsNaN())
            {
                throw GenerateDetailedInvalidTransactionException($"Balance delta calculated for account resource {key} is NaN");
            }

            await AddHistoryForCurrentTransaction(
                _dbContext.AccountResourceBalanceHistoryEntries,
                AccountResourceBalanceHistory.Matches(key),
                oldHistory =>
                {
                    var newHistoryEntry = AccountResourceBalanceHistory.FromPreviousHistory(key, oldHistory, entry);
                    if (newHistoryEntry.Balance.IsNegative())
                    {
                        throw GenerateDetailedInvalidTransactionException($"{key} balance ended up negative: {newHistoryEntry.Balance}");
                    }

                    return newHistoryEntry;
                });
        }
    }

    /// <summary>
    /// Handles Substates being booted up or shut down.
    /// </summary>
    /// <param name="substates">The DbSet of substates.</param>
    /// <param name="createNewPartialSubstate">Creates a new partial substate if it were booted up. Does not need to set the operation group reference.</param>
    /// <param name="verifyDownedSubstateMatchesExisting">Verifies if an existing substate matches, to perform sanity checking when downing a substate.</param>
    /// <typeparam name="TSubstate">The substate type.</typeparam>
    private Task HandleSubstateUpOrDown<TSubstate>(
        DbSet<TSubstate> substates,
        Func<TSubstate> createNewPartialSubstate,
        Func<TSubstate, bool> verifyDownedSubstateMatchesExisting
    )
        where TSubstate : SubstateBase
    {
        switch (_operation!.Substate.SubstateOperation)
        {
            case Substate.SubstateOperationEnum.BOOTUP:
                return HandleSubstateUp(substates, createNewPartialSubstate());
            case Substate.SubstateOperationEnum.SHUTDOWN:
                return HandleSubstateDown(substates, createNewPartialSubstate, verifyDownedSubstateMatchesExisting);
            default:
                throw GenerateDetailedInvalidTransactionException(
                    $"Unknown substate operation type: {_operation!.Substate.SubstateOperation}"
                );
        }
    }

    private Task HandleSubstateUp<TSubstate>(
        DbSet<TSubstate> substates,
        TSubstate newSubstate
    )
        where TSubstate : SubstateBase
    {
        // We capture the closure of the current transactionOpLocator
        //  as the action is async / will complete later, so we can't rely on the state of this class
        return substates.UpSubstate(
            GetCurrentTransactionOpLocator(),
            _operation!.Substate.SubstateIdentifier.Identifier.ConvertFromHex(),
            newSubstate,
            _dbOperationGroup!,
            _operationIndexInGroup!.Value,
            _cancellationToken
        );
    }

    private Task HandleSubstateDown<TSubstate>(
        DbSet<TSubstate> substates,
        Func<TSubstate> createNewPartialSubstate,
        Func<TSubstate, bool> verifySubstateMatches
    )
        where TSubstate : SubstateBase
    {
        // We capture the closure of the current transactionOpLocator
        //  as the action is async / will complete later, so we can't rely on the state of this class
        return substates.DownSubstate(
            GetCurrentTransactionOpLocator(),
            _operation!.Substate.SubstateIdentifier.Identifier.ConvertFromHex(),
            createNewPartialSubstate,
            verifySubstateMatches,
            _dbOperationGroup!,
            _operationIndexInGroup!.Value,
            _cancellationToken
        );
    }

    private Task AddHistoryForCurrentTransaction<THistory>(
        DbSet<THistory> history,
        Expression<Func<THistory, bool>> historySelector,
        Func<THistory?, THistory> createNewHistory
    )
        where THistory : HistoryBase
    {
        // We capture the closure of the current transactionOpLocator
        //  as the action is async / will complete later, so we can't rely on the state of this class
        return history.AddNewHistoryEntry(
            historySelector,
            createNewHistory,
            _transaction!.CommittedStateIdentifier.StateVersion,
            _cancellationToken
        );
    }

    private InvalidTransactionException GenerateDetailedInvalidTransactionException(string message)
    {
        return new InvalidTransactionException(GetCurrentTransactionOpLocator(), message);
    }

    private TransactionOpLocator GetCurrentTransactionOpLocator()
    {
        return new TransactionOpLocator(
            _transactionSummary!.StateVersion,
            _transaction!.TransactionIdentifier.Hash,
            _operationGroupIndex,
            _operationIndexInGroup
        );
    }
}
