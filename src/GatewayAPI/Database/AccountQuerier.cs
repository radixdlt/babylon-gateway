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
using Common.Extensions;
using Microsoft.EntityFrameworkCore;
using RadixGatewayApi.Generated.Model;
using Api = RadixGatewayApi.Generated.Model;

namespace GatewayAPI.Database;

public interface IAccountQuerier
{
    Task<AccountBalances> GetAccountBalancesAtState(string accountAddress, LedgerState ledgerState);
}

public class AccountQuerier : IAccountQuerier
{
    private readonly GatewayReadOnlyDbContext _dbContext;

    public AccountQuerier(GatewayReadOnlyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AccountBalances> GetAccountBalancesAtState(string accountAddress, LedgerState ledgerState)
    {
        return new AccountBalances(
            new TokenAmount("0", new TokenIdentifier("todo")),
            await GetAccountResourceBalancesAtState(ledgerState, accountAddress)
        );
    }

    private async Task<List<TokenAmount>> GetAccountResourceBalancesAtState(LedgerState ledgerState, string accountAddress)
    {
        return await _dbContext.AccountResourceBalanceHistoryEntries
            .GetHistoryEntryAtVersion(arb => arb.Account.Address == accountAddress, ledgerState._Version)
            .Include(arb => arb.Resource)
            .Select(x => new TokenAmount(
                x.BalanceEntry.Balance.ToSubUnitString(),
                new TokenIdentifier(x.Resource.ResourceIdentifier)
            ))
            .ToListAsync();
    }

    private async Task<TokenAmount> GetAccountTotalStakeBalance(LedgerState ledgerState, string accountAddress)
    {
        // TODO - make this much nicer(!)
        var accountValidatorsLatestVersion = await (
            from accountValidatorHistory in _dbContext.AccountValidatorStakeHistoryEntries
            where accountValidatorHistory.Account.Address == accountAddress
            group accountValidatorHistory by accountValidatorHistory.Validator
            into g
            select new
            {
                ValidatorId = g.Key.Id,
                AccountId = g.First().AccountId,
                LatestUpdateStateVersion = g.Select(h => h.FromStateVersion).Max(),
            }
        ).ToListAsync();

        var validatorsLatestVersion = await (
            from validatorStakeHistory in _dbContext.ValidatorStakeHistoryEntries
            where accountValidatorsLatestVersion.Select(v => v.ValidatorId).Contains(validatorStakeHistory.ValidatorId)
            group validatorStakeHistory by validatorStakeHistory.ValidatorId
            into g
            select new
            {
                ValidatorId = g.Key,
                LatestUpdateStateVersion = g.Select(h => h.FromStateVersion).Max(),
            }
        ).ToListAsync();

        var validatorStake = await _dbContext.ValidatorStakeHistoryEntries
            .FromSqlRawWithDimensionalIn(
                "SELECT * FROM validator_stake_history WHERE (validator_id, from_state_version)",
                validatorsLatestVersion,
                v => new object[] { v.ValidatorId, v.LatestUpdateStateVersion }
            )
            .ToDictionaryAsync(
                h => h.ValidatorId
            );

        var accountValidatorStake = await _dbContext.AccountValidatorStakeHistoryEntries
            .FromSqlRawWithDimensionalIn(
                "SELECT * FROM account_validator_stake_history WHERE (account_id, validator_id, from_state_version)",
                accountValidatorsLatestVersion,
                v => new object[] { v.AccountId, v.ValidatorId, v.LatestUpdateStateVersion }
            )
            .ToListAsync();

        // QQ - do aggregation!
        return new TokenAmount("0", new TokenIdentifier("todo"));
    }
}
