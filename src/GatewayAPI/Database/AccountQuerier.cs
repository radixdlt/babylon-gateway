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

using Common.Database;
using Common.Database.Models.Ledger.History;
using Common.Database.Models.Ledger.Substates;
using GatewayAPI.ApiSurface;
using GatewayAPI.Services;
using Microsoft.EntityFrameworkCore;
using RadixGatewayApi.Generated.Model;
using Api = RadixGatewayApi.Generated.Model;

namespace GatewayAPI.Database;

public interface IAccountQuerier
{
    Task<AccountBalances> GetAccountBalancesAtState(string accountAddress, LedgerState ledgerState);

    Task<AccountStakesResponse> GetStakePositionsAtState(string accountAddress, LedgerState ledgerState);

    Task<AccountUnstakesResponse> GetUnstakePositionsAtState(string accountAddress, LedgerState ledgerState);
}

public class AccountQuerier : IAccountQuerier
{
    private readonly GatewayReadOnlyDbContext _dbContext;
    private readonly INetworkConfigurationProvider _networkConfigurationProvider;

    public AccountQuerier(GatewayReadOnlyDbContext dbContext, INetworkConfigurationProvider networkConfigurationProvider)
    {
        _dbContext = dbContext;
        _networkConfigurationProvider = networkConfigurationProvider;
    }

    // TODO:NG-56 - Look up AccountId at ledger version and use that in lookups (like we do in ValidatorQuerier)
    // This can allow skipping a lot of work for unseen account ids; and slightly more efficient querying
    public async Task<AccountBalances> GetAccountBalancesAtState(string accountAddress, LedgerState ledgerState)
    {
        var account = await _dbContext.Account(accountAddress, ledgerState._Version).SingleOrDefaultAsync();

        if (account == null)
        {
            return new AccountBalances(
                new TokenAmount("0", _networkConfigurationProvider.GetXrdTokenIdentifier()),
                new List<TokenAmount>()
            );
        }

        return new AccountBalances(
            await GetAccountTotalStakeBalance(account.Id, ledgerState),
            await GetAccountResourceBalancesAtState(account.Id, ledgerState)
        );
    }

    public async Task<AccountStakesResponse> GetStakePositionsAtState(string accountAddress, LedgerState ledgerState)
    {
        var account = await _dbContext.Account(accountAddress, ledgerState._Version).SingleOrDefaultAsync();

        if (account == null)
        {
            return new AccountStakesResponse(
                ledgerState,
                new List<AccountStakeEntry>(),
                new List<AccountStakeEntry>()
            );
        }

        var allStakes = await GetAccountValidatorCombinedStakeSnapshots(account.Id, ledgerState).ToListAsync();

        return new AccountStakesResponse(
            ledgerState,
            MapValidatorStakes(
                allStakes.Select(x => new StakePosition(
                    x.ValidatorAddress,
                    x.AccountValidatorStakeSnapshot.TotalPreparedXrdStake
                ))
            ),
            MapValidatorStakes(
                allStakes.Select(x => new StakePosition(
                    x.ValidatorAddress,
                    x.ValidatorStakeSnapshot.EstimateXrdConversion(x.AccountValidatorStakeSnapshot.TotalStakeUnits)
                ))
            )
        );
    }

    public async Task<AccountUnstakesResponse> GetUnstakePositionsAtState(string accountAddress, LedgerState ledgerState)
    {
        var account = await _dbContext.Account(accountAddress, ledgerState._Version).SingleOrDefaultAsync();

        if (account == null)
        {
            return new AccountUnstakesResponse(
                ledgerState,
                new List<AccountUnstakeEntry>(),
                new List<AccountUnstakeEntry>()
            );
        }

        var allStakes = await GetAccountValidatorCombinedStakeSnapshots(account.Id, ledgerState).ToListAsync();

        return new AccountUnstakesResponse(
            ledgerState,
            MapValidatorUnstakes(
                allStakes.Select(x => new UnstakePosition(
                    x.ValidatorAddress,
                    x.ValidatorStakeSnapshot.EstimateXrdConversion(x.AccountValidatorStakeSnapshot.TotalPreparedUnStakeUnits),
                    501 // TODO:NG-57 - Fix Epochs Until Unlocked to be determined by engine configuration from Core API
                ))
            ),
            MapValidatorUnstakes(
                await GetExitingUnstakes(account.Id, ledgerState)
            )
        );
    }

    private async Task<List<TokenAmount>> GetAccountResourceBalancesAtState(long accountId, LedgerState ledgerState)
    {
        var balances = await _dbContext.AccountResourceBalanceHistoryAtVersion(ledgerState._Version)
            .Where(arb => arb.AccountId == accountId)
            .Include(arb => arb.Resource)
            .ToListAsync();

        return balances
            .Where(x => x.BalanceEntry.Balance.IsPositive())
            .OrderByDescending(x => x.BalanceEntry.Balance)
            .Select(x => new TokenAmount(
                x.BalanceEntry.Balance.ToSubUnitString(),
                new TokenIdentifier(x.Resource.ResourceIdentifier)
            ))
            .ToList();
    }

    private async Task<TokenAmount> GetAccountTotalStakeBalance(long accountId, LedgerState ledgerState)
    {
        var allStakes = await GetAccountValidatorCombinedStakeSnapshots(accountId, ledgerState).ToListAsync();
        var totalXrd = Common.Numerics.TokenAmount.Zero;

        foreach (var s in allStakes)
        {
            var accountValidatorStakeSnapshot = s.AccountValidatorStakeSnapshot;
            var validatorStakeSnapshot = s.ValidatorStakeSnapshot;

            var definiteXrd = accountValidatorStakeSnapshot.TotalPreparedXrdStake
                              + accountValidatorStakeSnapshot.TotalExitingXrdStake;
            var stakeUnits = accountValidatorStakeSnapshot.TotalStakeUnits
                                 + accountValidatorStakeSnapshot.TotalPreparedUnStakeUnits;

            totalXrd += definiteXrd + validatorStakeSnapshot.EstimateXrdConversion(stakeUnits);
        }

        return new TokenAmount(totalXrd.ToSubUnitString(), _networkConfigurationProvider.GetXrdTokenIdentifier());
    }

    private record StakePosition(string ValidatorAddress, Common.Numerics.TokenAmount Xrd);

    private List<AccountStakeEntry> MapValidatorStakes(IEnumerable<StakePosition> stakes)
    {
        return stakes
            .Where(s => s.Xrd.IsPositive())
            .OrderByDescending(s => s.Xrd)
            .Select(s => new AccountStakeEntry(
                s.ValidatorAddress.AsValidatorIdentifier(),
                s.Xrd.AsApiTokenAmount(_networkConfigurationProvider.GetXrdTokenIdentifier())
            ))
            .ToList();
    }

    private record UnstakePosition(string ValidatorAddress, Common.Numerics.TokenAmount Xrd, long EpochsUntilUnlocked);

    private List<AccountUnstakeEntry> MapValidatorUnstakes(IEnumerable<UnstakePosition> stakes)
    {
        return stakes
            .Where(s => s.Xrd.IsPositive())
            .OrderByDescending(s => s.Xrd)
            .Select(s => new AccountUnstakeEntry(
                s.ValidatorAddress.AsValidatorIdentifier(),
                s.Xrd.AsApiTokenAmount(_networkConfigurationProvider.GetXrdTokenIdentifier()),
                s.EpochsUntilUnlocked
            ))
            .ToList();
    }

    private async Task<IEnumerable<UnstakePosition>> GetExitingUnstakes(long accountId, LedgerState ledgerState)
    {
        var stateVersion = ledgerState._Version;

        var exitingUnstakes = await (
            from exitingStakeSubstates in _dbContext.AccountXrdStakeBalanceSubstates.UpAtVersion(stateVersion)
            join validator in _dbContext.Validators
                on exitingStakeSubstates.ValidatorId equals validator.Id
            where
                exitingStakeSubstates.AccountId == accountId &&
                exitingStakeSubstates.Type == AccountXrdStakeBalanceSubstateType.ExitingStake
            select new { ValidatorAddress = validator.Address, XrdAmount = exitingStakeSubstates.Amount, UnlockEpoch = exitingStakeSubstates.UnlockEpoch }
        ).ToListAsync();

        return exitingUnstakes
            .GroupBy(s => (s.ValidatorAddress, s.UnlockEpoch))
            .Select(g =>
            {
                var (validatorAddress, unlockEpoch) = g.Key;
                return new UnstakePosition(
                    validatorAddress,
                    g.Select(s => s.XrdAmount).Aggregate((a, b) => a + b),
                    unlockEpoch.HasValue ? unlockEpoch.Value - ledgerState.Epoch : 0 // Shouldn't ever be null, but protect just in case
                );
            });
    }

    private record CombinedStakeSnapshot(
        string ValidatorAddress,
        AccountValidatorStakeSnapshot AccountValidatorStakeSnapshot,
        ValidatorStakeSnapshot ValidatorStakeSnapshot
    );

    private IQueryable<CombinedStakeSnapshot> GetAccountValidatorCombinedStakeSnapshots(long accountId, LedgerState ledgerState)
    {
        var stateVersion = ledgerState._Version;

        return
            from stakeHistory in _dbContext.AccountValidatorStakeHistoryAtVersion(stateVersion)
            join validatorHistory in _dbContext.ValidatorStakeHistoryAtVersion(stateVersion)
                on stakeHistory.ValidatorId equals validatorHistory.ValidatorId
            join validator in _dbContext.Validators
                on stakeHistory.ValidatorId equals validator.Id
            where stakeHistory.AccountId == accountId
            select new CombinedStakeSnapshot(validator.Address, stakeHistory.StakeSnapshot, validatorHistory.StakeSnapshot)
        ;
    }
}
