/* Copyright 2021 Radix Publishing Ltd incorporated in Jersey (Channel Islands).
 *
 * Licensed under the Radix License, Version 1.0 (the "License"); you may not use this
 * file except in compliance with the License. You may obtain a copy of the License at:
 *
 * radixfoundation.org/licenses/LICENSE-v1
 *
 * The Licensor hereby grants permission for the Canonical version of the Work to be
 * published, distributed and used under or by reference to the Licensor’s trademark
 * Radix ® and use of any unregistered trade names, logos or get-up.
 *
 * The Licensor provides the Work (and each Contributor provides its Contributions) on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied,
 * including, without limitation, any warranties or conditions of TITLE, NON-INFRINGEMENT,
 * MERCHANTABILITY, or FITNESS FOR A PARTICULAR PURPOSE.
 *
 * Whilst the Work is capable of being deployed, used and adopted (instantiated) to create
 * a distributed ledger it is your responsibility to test and validate the code, together
 * with all logic and performance of that code under all foreseeable scenarios.
 *
 * The Licensor does not make or purport to make and hereby excludes liability for all
 * and any representation, warranty or undertaking in any form whatsoever, whether express
 * or implied, to any entity or person, including any representation, warranty or
 * undertaking, as to the functionality security use, value or other characteristics of
 * any distributed ledger nor in respect the functioning or value of any tokens which may
 * be created stored or transferred using the Work. The Licensor does not warrant that the
 * Work or any use of the Work complies with any law or regulation in any territory where
 * it may be implemented or used or that it will be appropriate for any specific purpose.
 *
 * Neither the licensor nor any current or former employees, officers, directors, partners,
 * trustees, representatives, agents, advisors, contractors, or volunteers of the Licensor
 * shall be liable for any direct or indirect, special, incidental, consequential or other
 * losses of any kind, in tort, contract or otherwise (including but not limited to loss
 * of revenue, income or profits, or loss of use or data, or loss of reputation, or loss
 * of any economic or other opportunity of whatsoever nature or howsoever arising), arising
 * out of or in connection with (without limitation of any use, misuse, of any ledger system
 * or use made or its functionality or any performance or operation of any code or protocol
 * caused by bugs or programming or logic errors or otherwise);
 *
 * A. any offer, purchase, holding, use, sale, exchange or transmission of any
 * cryptographic keys, tokens or assets created, exchanged, stored or arising from any
 * interaction with the Work;
 *
 * B. any failure in a transmission or loss of any token or assets keys or other digital
 * artefacts due to errors in transmission;
 *
 * C. bugs, hacks, logic errors or faults in the Work or any communication;
 *
 * D. system software or apparatus including but not limited to losses caused by errors
 * in holding or transmitting tokens by any third-party;
 *
 * E. breaches or failure of security including hacker attacks, loss or disclosure of
 * password, loss of private key, unauthorised use or misuse of such passwords or keys;
 *
 * F. any losses including loss of anticipated savings or other benefits resulting from
 * use of the Work or any changes to the Work (however implemented).
 *
 * You are solely responsible for; testing, validating and evaluation of all operation
 * logic, functionality, security and appropriateness of using the Work for any commercial
 * or non-commercial purpose and for any reproduction or redistribution by You of the
 * Work. You assume all risks associated with Your use of the Work and the exercise of
 * permissions under this License.
 */

using Common.Database.Models.Ledger;
using Common.Database.Models.Ledger.History;
using Common.Database.Models.Ledger.Substates;
using Common.Extensions;
using Common.Numerics;
using DataAggregator.DependencyInjection;
using DataAggregator.Exceptions;
using DataAggregator.Extensions;
using DataAggregator.GlobalServices;
using RadixCoreApi.GeneratedClient.Model;

namespace DataAggregator.LedgerExtension;

/// <summary>
/// A short-lived stateful class for extracting the content of a transaction.
/// </summary>
public class TransactionContentProcessor
{
    /* Dependencies */
    private readonly AggregatorDbContext _dbContext;
    private readonly DbActionsPlanner _dbActionsPlanner;
    private readonly IEntityDeterminer _entityDeterminer;

    /* History */
    private readonly Dictionary<AccountResource, TokenAmount> _accountResourceNetBalanceChanges = new();

    /* Mutable Class State */
    /* > These simply help us avoid passing tons of references down the call stack.
    /* > These will all not be null at the time of use in the Handle methods. */
    private TransactionSummary? _transactionSummary;
    private CommittedTransaction? _transaction;
    private LedgerOperationGroup? _dbOperationGroup;
    private OperationGroup? _transactionOperationGroup;
    private int _operationGroupIndex = -1;
    private Operation? _operation;
    private int _operationIndexInGroup = -1;
    private Entity? _entity;
    private TokenAmount? _amount;

    public TransactionContentProcessor(AggregatorDbContext dbContext, DbActionsPlanner dbActionsPlanner, IEntityDeterminer entityDeterminer)
    {
        _dbContext = dbContext;
        _dbActionsPlanner = dbActionsPlanner;
        _entityDeterminer = entityDeterminer;
    }

    public void ProcessTransactionContents(CommittedTransaction transaction, TransactionSummary transactionSummary)
    {
        _transaction = transaction;
        _transactionSummary = transactionSummary;
        _operationGroupIndex = -1;
        foreach (var operationGroup in transaction.OperationGroups)
        {
            _operationGroupIndex++;
            if (!operationGroup.HasSubstantiveOperations())
            {
                continue;
            }

            _operationIndexInGroup = -1;
            _transactionOperationGroup = operationGroup;
            _dbOperationGroup = new LedgerOperationGroup(
                transaction.CommittedStateIdentifier.StateVersion,
                _operationGroupIndex,
                null // TODO:NG-41 - fix inferred actions
            );
            _dbContext.OperationGroups.Add(_dbOperationGroup);

            foreach (var operation in _transactionOperationGroup.Operations)
            {
                _operationIndexInGroup += 1;
                if (!operation.IsNotRoundDataOrValidatorBftData())
                {
                    continue;
                }

                _operation = operation;
                _entity = _entityDeterminer.DetermineEntity(_operation.EntityIdentifier);
                if (_entity == null)
                {
                    throw GenerateDetailedInvalidTransactionException(
                        $"Failed to identify the entity"
                    );
                }

                HandleOperation();

                _entity = null;
                _amount = null;
                _operation = null;
            }
        }

        _dbOperationGroup = null;
        _transactionOperationGroup = null;
        _operationGroupIndex = -1;
        _operationIndexInGroup = -1;

        HandleHistoryUpdates();
    }

    private void HandleOperation()
    {
        if (_operation!.Amount != null)
        {
            HandleAmountOperation();
        }

        if (_operation!.Data != null)
        {
            HandleDataOperation();
        }
    }

    private void HandleAmountOperation()
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
                HandleResourceAmountOperation(resourceIdentifier);
                return;
            case StakeOwnershipResourceIdentifier stakeOwnershipResourceIdentifier:
                HandleStakeOwnershipAmountOperation(stakeOwnershipResourceIdentifier);
                return;
            default:
                throw GenerateDetailedInvalidTransactionException(
                    $"Unknown resource identifier type: {_operation!.Amount.ResourceIdentifier.Type}"
                );
        }
    }

    private void HandleResourceAmountOperation(TokenResourceIdentifier resourceIdentifier)
    {
        switch (_entity!.EntityType)
        {
            case EntityType.Account:
                HandleAccountResourceAmountOperation(_entity!.AccountAddress!, resourceIdentifier.Rri);
                return;
            case EntityType.Account_ExitingStake:
            case EntityType.Account_PreparedStake:
                HandleAccountXrdStakeResourceAmountOperation(_entity!, resourceIdentifier.Rri);
                return;
            case EntityType.Validator_System:
                HandleValidatorResourceAmountOperation(_entity!.ValidatorAddress!, resourceIdentifier.Rri);
                return;
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

    private void HandleStakeOwnershipAmountOperation(StakeOwnershipResourceIdentifier stakeOwnershipResourceIdentifier)
    {
        switch (_entity!.EntityType)
        {
            case EntityType.Account:
            case EntityType.Account_PreparedUnstake:
                HandleAccountStakeOwnershipAmountOperation(_entity!.EntityType, _entity!.AccountAddress!, stakeOwnershipResourceIdentifier.Validator);
                return;
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

    private void HandleAccountResourceAmountOperation(string accountAddress, string resourceIdentifier)
    {
        var accountResource = new AccountResource(accountAddress, resourceIdentifier);
        var tokenAmount = _amount!.Value;

        // Part 1) Handle substates
        HandleSubstateUpOrDown(
            () => new AccountResourceBalanceSubstate(accountResource, tokenAmount),
            existingSubstate => (
                existingSubstate.AccountAddress == accountAddress
                && existingSubstate.ResourceIdentifier == resourceIdentifier
                && existingSubstate.Amount == -tokenAmount // Negative because downed has the opposite amount as upped
            )
        );

        // Part 2) Handle history
        _accountResourceNetBalanceChanges.TrackBalanceDelta(accountResource, TokenAmount.FromSubUnitsString(_operation!.Amount.Value));
    }

    private void HandleAccountXrdStakeResourceAmountOperation(Entity entity, string resourceIdentifier)
    {
        var tokenAmount = _amount!.Value;

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
        HandleSubstateUpOrDown(
            () => new AccountXrdStakeBalanceSubstate(
                entity.AccountAddress!,
                entity.ValidatorAddress!,
                type,
                entity.EpochUnlock,
                tokenAmount
            ),
            existingSubstate => (
                existingSubstate.AccountAddress == entity.AccountAddress!
                && existingSubstate.ValidatorAddress == entity.ValidatorAddress!
                && existingSubstate.Type == type
                && existingSubstate.UnlockEpoch == entity.EpochUnlock
                && existingSubstate.Amount == -tokenAmount // Negative because downed has the opposite amount as upped
            )
        );

        // No history in this case
    }

    private void HandleValidatorResourceAmountOperation(string validatorAddress, string resourceIdentifier)
    {
        var tokenAmount = _amount!.Value;

        if (!_entityDeterminer.IsXrd(resourceIdentifier))
        {
            throw GenerateDetailedInvalidTransactionException(
                $"Resource Amount operation against unsupported entity type: {_entity!.EntityType}"
            );
        }

        // Part 1) Handle substates
        HandleSubstateUpOrDown(
            () => new ValidatorStakeBalanceSubstate(
                validatorAddress,
                _transactionSummary!.Epoch, // Put in for history's sake, but not part of the substate, so shouldn't be verified against
                tokenAmount
            ),
            existingSubstate => (
                existingSubstate.ValidatorAddress == validatorAddress
                && existingSubstate.Amount == -tokenAmount // Negative because downed has the opposite amount as upped
            )
        );

        // No history in this case
    }

    private void HandleAccountStakeOwnershipAmountOperation(EntityType entityType, string accountAddress, string validatorAddress)
    {
        var tokenAmount = _amount!.Value;

        var type = entityType switch
        {
            EntityType.Account => AccountStakeOwnershipBalanceSubstateType.Stake,
            EntityType.Account_PreparedUnstake => AccountStakeOwnershipBalanceSubstateType.PreparedUnstake,
            _ => throw new ArgumentOutOfRangeException(),
        };

        // Part 1) Handle substates
        HandleSubstateUpOrDown(
            () => new AccountStakeOwnershipBalanceSubstate(
                accountAddress,
                validatorAddress,
                type,
                tokenAmount
            ),
            existingSubstate => (
                existingSubstate.AccountAddress == accountAddress
                && existingSubstate.ValidatorAddress == validatorAddress
                && existingSubstate.Type == type
                && existingSubstate.Amount == -tokenAmount // Negative because downed has the opposite amount as upped
            )
        );

        // No history yet...
    }

    private void HandleDataOperation()
    {
        // TODO:NG-24
        return;
    }

    private void HandleHistoryUpdates()
    {
        HandleAccountResourceBalanceHistoryUpdates();
    }

    private void HandleAccountResourceBalanceHistoryUpdates()
    {
        var accountResourceHistoryChanges = _accountResourceNetBalanceChanges
            .Where(arnb => arnb.Value != TokenAmount.Zero);

        foreach (var (key, entry) in accountResourceHistoryChanges)
        {
            if (entry.IsNaN())
            {
                throw GenerateDetailedInvalidTransactionException($"Balance delta calculated for account resource {key} is NaN");
            }

            _dbActionsPlanner.AddNewAccountResourceBalanceHistoryEntry(
                key,
                oldHistory =>
                {
                    var newHistoryEntry = AccountResourceBalanceHistory.FromPreviousHistory(key, oldHistory, entry);
                    if (newHistoryEntry.Balance.IsNegative())
                    {
                        throw GenerateDetailedInvalidTransactionException($"{key} balance ended up negative: {newHistoryEntry.Balance}");
                    }

                    return newHistoryEntry;
                },
                _transactionSummary!.StateVersion
            );
        }
    }

    /// <summary>
    /// Handles Substates being booted up or shut down.
    /// </summary>
    /// <param name="createNewPartialSubstate">Creates a new partial substate if it were booted up. Does not need to set the operation group reference.</param>
    /// <param name="verifyDownedSubstateMatchesExisting">Verifies if an existing substate matches, to perform sanity checking when downing a substate.</param>
    /// <typeparam name="TSubstate">The substate type.</typeparam>
    private void HandleSubstateUpOrDown<TSubstate>(
        Func<TSubstate> createNewPartialSubstate,
        Func<TSubstate, bool> verifyDownedSubstateMatchesExisting
    )
        where TSubstate : SubstateBase
    {
        switch (_operation!.Substate.SubstateOperation)
        {
            case Substate.SubstateOperationEnum.BOOTUP:
                HandleSubstateUp(createNewPartialSubstate());
                return;
            case Substate.SubstateOperationEnum.SHUTDOWN:
                HandleSubstateDown(createNewPartialSubstate, verifyDownedSubstateMatchesExisting);
                return;
            default:
                throw GenerateDetailedInvalidTransactionException(
                    $"Unknown substate operation type: {_operation!.Substate.SubstateOperation}"
                );
        }
    }

    private void HandleSubstateUp<TSubstate>(
        TSubstate newSubstate
    )
        where TSubstate : SubstateBase
    {
        _dbActionsPlanner.UpSubstate(
            GetCurrentTransactionOpLocator(),
            _operation!.Substate.SubstateIdentifier.Identifier.ConvertFromHex(),
            newSubstate,
            _dbOperationGroup!,
            _operationIndexInGroup
        );
    }

    private void HandleSubstateDown<TSubstate>(
        Func<TSubstate> createNewPartialSubstate,
        Func<TSubstate, bool> verifySubstateMatches
    )
        where TSubstate : SubstateBase
    {
        _dbActionsPlanner.DownSubstate(
            GetCurrentTransactionOpLocator(),
            _operation!.Substate.SubstateIdentifier.Identifier.ConvertFromHex(),
            createNewPartialSubstate,
            verifySubstateMatches,
            _dbOperationGroup!,
            _operationIndexInGroup
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
