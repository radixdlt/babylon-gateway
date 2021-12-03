using Common.Database.Models.Ledger;
using Common.Database.Models.Ledger.History;
using Common.Numerics;
using DataAggregator.Exceptions;
using DataAggregator.GlobalServices;
using RadixCoreApi.GeneratedClient.Model;
using TokenData = RadixCoreApi.GeneratedClient.Model.TokenData;

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

    public InferredAction? InferAction(bool isSystemTransaction, TransactionOpLocator transactionOpLocator, Func<string, ValidatorStakeSnapshot> validatorStakeLookup)
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
            return InferTokenCreationAction(transactionOpLocator, withdrawals, deposits);
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

        if (withdrawals.Count == 0)
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
                fromAddress: null,
                toAddress: entity.AccountAddress!,
                amount: mint.Delta,
                resourceIdentifier: rri
            );
        }

        if (deposits.Count == 0)
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
                fromAddress: null,
                toAddress: entity.AccountAddress!,
                amount: -burn.Delta,
                resourceIdentifier: rri
            );
        }

        // Exactly one sender and receiver
        var sender = withdrawals.First().Entity;
        var recipient = deposits.First().Entity;
        var sentResource = withdrawals.First().ResourceIdentifier;
        var receivedResource = deposits.First().ResourceIdentifier;
        var sentAmount = withdrawals.First().Delta;
        var receivedAmount = -deposits.First().Delta;

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
                fromAddress: sender.AccountAddress!,
                toAddress: recipient.ValidatorAddress!,
                amount: sentAmount,
                resourceIdentifier: _entityDeterminer.GetXrdAddress()
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

            var xrdEstimate = validatorStakeLookup(stakeUnitResourceIdentifier.ValidatorAddress).EstimateXrdConversion(sentAmount);

            return new InferredAction(
                InferredActionType.UnstakeTokens,
                fromAddress: sender.AccountAddress!,
                toAddress: recipient.ValidatorAddress!,
                amount: xrdEstimate,
                resourceIdentifier: _entityDeterminer.GetXrdAddress()
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
                fromAddress: sender.AccountAddress!,
                toAddress: recipient.AccountAddress!,
                amount: sentAmount,
                resourceIdentifier: tokenResourceIdentifier.Rri
            );
        }

        return null;
    }

    // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
    private InferredAction InferTokenCreationAction(
        TransactionOpLocator transactionOpLocator,
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
            fromAddress: null,
            toAddress: entity?.AccountAddress,
            resourceIdentifier: tokenData.Key.ResourceAddress!,
            amount: deposits.FirstOrDefault()?.Delta ?? TokenAmount.Zero
        );
    }
}
