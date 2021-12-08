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
using Common.Extensions;
using Common.Numerics;
using DataAggregator.Exceptions;
using DataAggregator.GlobalServices;
using RadixCoreApi.Generated.Model;
using TokenData = RadixCoreApi.Generated.Model.TokenData;

namespace DataAggregator.LedgerExtension;

public record AccountResourceBalanceChange(string AccountAddress, string ResourceAddress, TokenAmount BalanceChange);

/// <summary>
/// A stateful class to track changes (eg in a substate group, or across a transaction).
/// </summary>
public class Accounting
{
    private readonly IEntityDeterminer _entityDeterminer;
    private readonly Dictionary<(Entity Entity, ResourceIdentifier ResourceIdentifier), TokenAmount> _trackedTotals = new();
    private readonly Dictionary<Entity, TokenData> _tokenData = new();

    public Accounting(IEntityDeterminer entityDeterminer)
    {
        _entityDeterminer = entityDeterminer;
    }

    public void TrackDelta(Entity entity, ResourceIdentifier resource, TokenAmount tokenAmount)
    {
        _trackedTotals.TrackBalanceDelta((entity, resource), tokenAmount);
    }

    public void TrackTokenCreation(Entity entity, TokenData tokenData)
    {
        _tokenData[entity] = tokenData;
    }

    public Dictionary<(Entity Entity, ResourceIdentifier ResourceIdentifier), TokenAmount> GetAllTotalChanges()
    {
        return _trackedTotals;
    }

    public HashSet<string> GetReferencedAccountAddresses()
    {
        return _trackedTotals
            .Keys
            .Where(k => k.Entity.AccountAddress != null)
            .Select(k => k.Entity.AccountAddress!)
            .ToHashSet();
    }

    public List<AccountResourceBalanceChange> GetAccountResourceChanges()
    {
        return _trackedTotals
            .Where(t =>
                !t.Value.IsZero()
                && t.Key.Entity.EntityType == EntityType.Account
                && t.Key.ResourceIdentifier is TokenResourceIdentifier
            )
            .Select(t => new AccountResourceBalanceChange(
                t.Key.Entity.AccountAddress!,
                ((TokenResourceIdentifier)t.Key.ResourceIdentifier).Rri,
                t.Value
            ))
            .ToList();
    }

    public record AccountingEntry(Entity Entity, ResourceIdentifier ResourceIdentifier, TokenAmount Delta);

    public InferredAction? InferAction(bool isSystemTransaction, TransactionOpLocator transactionOpLocator, DbActionsPlanner dbActionsPlanner)
    {
        var withdrawals = _trackedTotals
            .Where(t => t.Value.IsNegative())
            .Select(t => new AccountingEntry(t.Key.Entity, t.Key.ResourceIdentifier, t.Value))
            .ToList();

        var deposits = _trackedTotals
            .Where(t => t.Value.IsPositive())
            .Select(t => new AccountingEntry(t.Key.Entity, t.Key.ResourceIdentifier, t.Value))
            .ToList();

        if (_tokenData.Any())
        {
            return InferTokenCreationAction(transactionOpLocator, dbActionsPlanner, withdrawals, deposits);
        }

        if (withdrawals.Count == 0 && deposits.Count == 0)
        {
            return null;
        }

        if (withdrawals.Count > 1)
        {
            if (isSystemTransaction)
            {
                return InferredAction.Complex();
            }

            throw new InvalidTransactionException(transactionOpLocator, "Invalid operation group in non-system transaction with multiple vault withdrawals");
        }

        if (deposits.Count > 1)
        {
            return InferredAction.Complex();
        }

        if (deposits.Count == 1 && withdrawals.Count == 0)
        {
            var mint = deposits.First();
            if (mint.ResourceIdentifier is not TokenResourceIdentifier tokenResourceIdentifier)
            {
                return null;
            }

            var rri = tokenResourceIdentifier.Rri;

            var entity = mint.Entity;
            if (entity.EntityType != EntityType.Account)
            {
                throw new InvalidTransactionException(transactionOpLocator, "Token minted to non-account");
            }

            return new InferredAction(
                _entityDeterminer.IsXrd(rri) ? InferredActionType.MintXrd : InferredActionType.MintTokens,
                fromAccount: null,
                toAccount: dbActionsPlanner.GetLoadedAccount(entity.AccountAddress!),
                validator: null,
                amount: mint.Delta,
                resource: dbActionsPlanner.GetLoadedResource(rri)
            );
        }

        if (withdrawals.Count == 1 && deposits.Count == 0)
        {
            var burn = withdrawals.First();
            if (burn.ResourceIdentifier is not TokenResourceIdentifier tokenResourceIdentifier)
            {
                return null;
            }

            var rri = tokenResourceIdentifier.Rri;

            var entity = burn.Entity;
            if (entity.EntityType != EntityType.Account)
            {
                throw new InvalidTransactionException(transactionOpLocator, "Token burned from non-account");
            }

            return new InferredAction(
                _entityDeterminer.IsXrd(rri) ? InferredActionType.PayXrd : InferredActionType.BurnTokens,
                fromAccount: dbActionsPlanner.GetLoadedAccount(entity.AccountAddress!),
                toAccount: null,
                validator: null,
                amount: -burn.Delta,
                resource: dbActionsPlanner.GetLoadedResource(rri)
            );
        }

        // Exactly one sender and receiver
        var sender = withdrawals.First().Entity;
        var recipient = deposits.First().Entity;
        var sentResource = withdrawals.First().ResourceIdentifier;
        var receivedResource = deposits.First().ResourceIdentifier;
        var sentAmount = -withdrawals.First().Delta;
        var receivedAmount = deposits.First().Delta;

        if (recipient.EntityType == EntityType.Account_PreparedStake)
        {
            if (sender.EntityType != EntityType.Account)
            {
                throw new InvalidTransactionException(transactionOpLocator, "Prepared stake from non-account");
            }

            if (sentResource is not TokenResourceIdentifier tokenResourceIdentifier || !_entityDeterminer.IsXrd(tokenResourceIdentifier.Rri))
            {
                throw new InvalidTransactionException(transactionOpLocator, "Prepared stake using non-xrd resource");
            }

            return new InferredAction(
                InferredActionType.StakeTokens,
                fromAccount: dbActionsPlanner.GetLoadedAccount(sender.AccountAddress!),
                toAccount: null,
                validator: dbActionsPlanner.GetLoadedValidator(recipient.ValidatorAddress!),
                amount: sentAmount,
                resource: dbActionsPlanner.GetLoadedResource(_entityDeterminer.GetXrdAddress())
            );
        }

        if (recipient.EntityType == EntityType.Account_PreparedUnstake)
        {
            if (sender.EntityType != EntityType.Account)
            {
                throw new InvalidTransactionException(transactionOpLocator, "Prepared stake from non-account");
            }

            if (!sentResource.Equals(receivedResource))
            {
                throw new InvalidTransactionException(transactionOpLocator, "Prepared unstake send and receive resources aren't equal");
            }

            if (sentAmount != receivedAmount)
            {
                throw new InvalidTransactionException(transactionOpLocator, "Prepared unstake send and receive amounts weren't equal StakeUnits");
            }

            if (sentResource is not StakeUnitResourceIdentifier stakeUnitResourceIdentifier)
            {
                throw new InvalidTransactionException(transactionOpLocator, "Prepared unstake using non-stake unit resource");
            }

            var xrdEstimate = dbActionsPlanner.GetLoadedLatestValidatorStakeSnapshot(stakeUnitResourceIdentifier.ValidatorAddress).EstimateXrdConversion(sentAmount);

            return new InferredAction(
                InferredActionType.UnstakeTokens,
                fromAccount: null,
                toAccount: dbActionsPlanner.GetLoadedAccount(sender.AccountAddress!),
                validator: dbActionsPlanner.GetLoadedValidator(stakeUnitResourceIdentifier.ValidatorAddress!),
                amount: xrdEstimate,
                resource: dbActionsPlanner.GetLoadedResource(_entityDeterminer.GetXrdAddress())
            );
        }

        if (sender.EntityType == EntityType.Account && recipient.EntityType == EntityType.Account)
        {
            if (sentAmount != receivedAmount)
            {
                throw new InvalidTransactionException(transactionOpLocator, "Transfer send and receive amounts weren't equal");
            }

            if (sentResource is not TokenResourceIdentifier tokenResourceIdentifier)
            {
                throw new InvalidTransactionException(transactionOpLocator, "Cannot transfer non-token");
            }

            return new InferredAction(
                InferredActionType.SimpleTransfer,
                fromAccount: dbActionsPlanner.GetLoadedAccount(sender.AccountAddress!),
                toAccount: dbActionsPlanner.GetLoadedAccount(recipient.AccountAddress!),
                validator: null,
                amount: sentAmount,
                resource: dbActionsPlanner.GetLoadedResource(tokenResourceIdentifier.Rri)
            );
        }

        return null;
    }

    // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
    private InferredAction InferTokenCreationAction(
        TransactionOpLocator transactionOpLocator,
        DbActionsPlanner dbActionsPlanner,
        List<AccountingEntry> withdrawals,
        List<AccountingEntry> deposits
    )
    {
        if (withdrawals.Count > 0)
        {
            throw new InvalidTransactionException(transactionOpLocator, "Should be no withdrawals in Token Creation");
        }

        if (deposits.Count > 1)
        {
            throw new InvalidTransactionException(transactionOpLocator, "Should be no more than 1 account credited with tokens during Token Creation");
        }

        if (_tokenData.Count > 1)
        {
            throw new InvalidTransactionException(transactionOpLocator, "There should be no more than one token data created in a given operation group");
        }

        var entity = deposits.FirstOrDefault()?.Entity;
        var tokenData = _tokenData.First();

        if (entity != null && entity.EntityType != EntityType.Account)
        {
            throw new InvalidTransactionException(transactionOpLocator, "Token creation credited to non-account");
        }

        return new InferredAction(
            InferredActionType.CreateTokenDefinition,
            fromAccount: null,
            toAccount: entity != null ? dbActionsPlanner.GetLoadedAccount(entity.AccountAddress!) : null,
            validator: null,
            amount: deposits.FirstOrDefault()?.Delta ?? TokenAmount.Zero,
            resource: dbActionsPlanner.GetLoadedResource(tokenData.Key.ResourceAddress!)
        );
    }
}
