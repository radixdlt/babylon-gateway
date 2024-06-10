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

using NpgsqlTypes;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using RadixDlt.NetworkGateway.PostgresIntegration.Services;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using ToolkitModel = RadixEngineToolkit;

namespace RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension;

internal class ValidatorEmissionProcessor
{
    private record Emission(long FromStateVersion, long ValidatorEntityId, long EpochNumber, long ProposalsMade, long ProposalsMissed);

    private readonly ProcessorContext _context;

    private Dictionary<long, ValidatorCumulativeEmissionHistory> _mostRecentCumulativeEmissions = new();

    private List<Emission> _emissions = new();
    private List<ValidatorCumulativeEmissionHistory> _cumulativeEmissionsToAdd = new();

    public ValidatorEmissionProcessor(ProcessorContext context)
    {
        _context = context;
    }

    public void VisitEvent(ToolkitModel.TypedNativeEvent decodedEvent, ReferencedEntity eventEmitterEntity, long stateVersion)
    {
        if (EventDecoder.TryGetValidatorEmissionsAppliedEvent(decodedEvent, out var validatorUptimeEvent))
        {
            _emissions.Add(new Emission(
                stateVersion,
                eventEmitterEntity.DatabaseId,
                (long)validatorUptimeEvent.epoch,
                (long)validatorUptimeEvent.proposalsMade,
                (long)validatorUptimeEvent.proposalsMissed));
        }
    }

    public async Task LoadDependencies()
    {
        _mostRecentCumulativeEmissions.AddRange(await MostRecentCumulativeEmissions());
    }

    public void ProcessChanges()
    {
        foreach (var emission in _emissions)
        {
            var proposalsMade = 0L;
            var proposalsMissed = 0L;
            var participationInActiveSet = 0L;

            if (_mostRecentCumulativeEmissions.TryGetValue(emission.ValidatorEntityId, out var previous))
            {
                proposalsMade = previous.ProposalsMade;
                proposalsMissed = previous.ProposalsMissed;
                participationInActiveSet = previous.ParticipationInActiveSet;
            }

            proposalsMade += emission.ProposalsMade;
            proposalsMissed += emission.ProposalsMissed;
            participationInActiveSet += 1;

            var cumulativeEmission = new ValidatorCumulativeEmissionHistory
            {
                Id = _context.Sequences.ValidatorCumulativeEmissionHistorySequence++,
                FromStateVersion = emission.FromStateVersion,
                ValidatorEntityId = emission.ValidatorEntityId,
                EpochNumber = emission.EpochNumber,
                ProposalsMade = proposalsMade,
                ProposalsMissed = proposalsMissed,
                ParticipationInActiveSet = participationInActiveSet,
            };

            _mostRecentCumulativeEmissions[cumulativeEmission.ValidatorEntityId] = cumulativeEmission;
            _cumulativeEmissionsToAdd.Add(cumulativeEmission);
        }
    }

    public async Task<int> SaveEntities()
    {
        var rowsInserted = 0;

        rowsInserted += await CopyCumulativeEmissionHistory();

        return rowsInserted;
    }

    private async Task<IDictionary<long, ValidatorCumulativeEmissionHistory>> MostRecentCumulativeEmissions()
    {
        var validatorEntityIds = _emissions.Select(e => e.ValidatorEntityId).ToHashSet().ToList();

        if (!validatorEntityIds.Any())
        {
            return ImmutableDictionary<long, ValidatorCumulativeEmissionHistory>.Empty;
        }

        return await _context.ReadHelper.LoadDependencies<long, ValidatorCumulativeEmissionHistory>(
            @$"
WITH variables (validator_entity_id) AS (
    SELECT UNNEST({validatorEntityIds})
)
SELECT vceh.*
FROM variables var
INNER JOIN LATERAL (
    SELECT *
    FROM validator_cumulative_emission_history
    WHERE validator_entity_id = var.validator_entity_id
    ORDER BY from_state_version DESC
    LIMIT 1
) vceh ON true;",
            e => e.ValidatorEntityId);
    }

    private Task<int> CopyCumulativeEmissionHistory() => _context.WriteHelper.Copy(
        _cumulativeEmissionsToAdd,
        "COPY validator_cumulative_emission_history (id, from_state_version, validator_entity_id, epoch_number, proposals_made, proposals_missed, participation_in_active_set) FROM STDIN (FORMAT BINARY)",
        async (writer, e, token) =>
        {
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.ValidatorEntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.EpochNumber, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.ProposalsMade, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.ProposalsMissed, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.ParticipationInActiveSet, NpgsqlDbType.Bigint, token);
        });
}
