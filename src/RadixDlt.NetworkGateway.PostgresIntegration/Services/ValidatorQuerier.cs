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

using Microsoft.EntityFrameworkCore;
using RadixDlt.NetworkGateway.Abstractions;
using RadixDlt.NetworkGateway.GatewayApi.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GatewayModel = RadixDlt.NetworkGateway.GatewayApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Services;

internal class ValidatorQuerier : IValidatorQuerier
{
    private readonly ReadOnlyDbContext _dbContext;

    public ValidatorQuerier(ReadOnlyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GatewayModel.ValidatorsUptimeResponse> ValidatorsUptimeStatistics(
        IList<EntityAddress> validatorAddresses,
        GatewayModel.LedgerState ledgerState,
        GatewayModel.LedgerState? fromLedgerState,
        CancellationToken token = default)
    {
        var addresses = validatorAddresses.Select(a => (string)a).ToHashSet().ToList();

        var validators = await _dbContext
            .Entities
            .Where(e => addresses.Contains(e.Address) && e.FromStateVersion <= ledgerState.StateVersion)
            .AnnotateMetricName("GetValidators")
            .ToDictionaryAsync(e => e.Id, e => e.Address, token);

        var validatorIds = validators.Keys.ToList();
        var epochFrom = fromLedgerState?.Epoch ?? 0;
        var epochTo = ledgerState.Epoch;

        var validatorUptime = await _dbContext
            .ValidatorCumulativeEmissionHistory
            .FromSqlInterpolated($@"
WITH variables AS (SELECT UNNEST({validatorIds}) AS validator_entity_id)
SELECT
    h.id,
    h.from_state_version,
    h.validator_entity_id,
    h.epoch_number,
    h.proposals_made - COALESCE(l.proposals_made, 0) AS proposals_made,
    h.proposals_missed - COALESCE(l.proposals_missed, 0) AS proposals_missed,
    h.participation_in_active_set - COALESCE(l.participation_in_active_set, 0) AS participation_in_active_set
FROM variables var
INNER JOIN LATERAL (
    SELECT *
    FROM validator_cumulative_emission_history
    WHERE validator_entity_id = var.validator_entity_id AND epoch_number <= {epochTo}
    ORDER BY epoch_number DESC
    LIMIT 1
) h ON TRUE
LEFT JOIN LATERAL (
    SELECT *
    FROM validator_cumulative_emission_history
    WHERE validator_entity_id = var.validator_entity_id AND epoch_number >= {epochFrom} AND epoch_number < h.epoch_number
    ORDER BY epoch_number
    LIMIT 1
) l ON TRUE")
            .AnnotateMetricName("ValidatorUptime")
            .ToDictionaryAsync(e => e.ValidatorEntityId, token);

        var items = validators
            .Select(v =>
            {
                long? proposalsMadeSum = null;
                long? proposalsMissedSum = null;
                long epochsActiveIn = 0;

                if (validatorUptime.TryGetValue(v.Key, out var uptime))
                {
                    proposalsMadeSum = uptime.ProposalsMade;
                    proposalsMissedSum = uptime.ProposalsMissed;
                    epochsActiveIn = uptime.ParticipationInActiveSet;
                }

                return new GatewayModel.ValidatorUptimeCollectionItem(v.Value, proposalsMadeSum, proposalsMissedSum, epochsActiveIn);
            })
            .ToList();

        return new GatewayModel.ValidatorsUptimeResponse(ledgerState, new GatewayModel.ValidatorUptimeCollection(items));
    }
}
