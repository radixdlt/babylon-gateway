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
using RadixDlt.NetworkGateway.GatewayApi.Services;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GatewayModel = RadixDlt.NetworkGateway.GatewayApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Services;

internal partial class EntityStateQuerier
{
    public async Task<GatewayModel.StatePackageBlueprintPageResponse?> PackageBlueprints(
        IEntityStateQuerier.PageRequest pageRequest,
        GatewayModel.LedgerState ledgerState,
        CancellationToken token = default)
    {
        var package = await GetEntity<GlobalPackageEntity>(pageRequest.Address, ledgerState, token);
        var packageBlueprintHistoryAndOneMore = await GetPackageBlueprintHistory(new[] { package.Id }, pageRequest.Offset, pageRequest.Limit + 1, ledgerState, token);

        if (!packageBlueprintHistoryAndOneMore.TryGetValue(package.Id, out var packageBlueprintsAndOneMore))
        {
            return null;
        }

        var correlatedAddresses = await GetCorrelatedEntityAddresses(new[] { package }, packageBlueprintHistoryAndOneMore, ledgerState, token);
        var items = packageBlueprintsAndOneMore.Take(pageRequest.Limit).Select(pb => pb.ToGatewayModel(correlatedAddresses)).ToList();
        var cursor = CursorGenerator.GenerateOffsetCursor(pageRequest.Offset, pageRequest.Limit, packageBlueprintsAndOneMore);

        return new GatewayModel.StatePackageBlueprintPageResponse(
            ledgerState: ledgerState,
            packageAddress: package.Address,
            items: items,
            nextCursor: cursor);
    }

    public async Task<GatewayModel.StatePackageCodePageResponse?> PackageCodes(IEntityStateQuerier.PageRequest pageRequest, GatewayModel.LedgerState ledgerState, CancellationToken token = default)
    {
        var package = await GetEntity<GlobalPackageEntity>(pageRequest.Address, ledgerState, token);
        var packageCodeHistoryAndOneMore = await GetPackageCodeHistory(new[] { package.Id }, pageRequest.Offset, pageRequest.Limit + 1, ledgerState, token);

        if (!packageCodeHistoryAndOneMore.TryGetValue(package.Id, out var packageCodesAndOneMore))
        {
            return null;
        }

        var items = packageCodesAndOneMore.Take(pageRequest.Limit).Select(pb => pb.ToGatewayModel()).ToList();
        var cursor = CursorGenerator.GenerateOffsetCursor(pageRequest.Offset, pageRequest.Limit, packageCodesAndOneMore);

        return new GatewayModel.StatePackageCodePageResponse(
            ledgerState: ledgerState,
            packageAddress: package.Address,
            items: items,
            nextCursor: cursor);
    }

    private async Task<IDictionary<long, PackageBlueprintHistory[]>> GetPackageBlueprintHistory(long[] packageEntityIds, int offset, int limit, GatewayModel.LedgerState ledgerState, CancellationToken token)
    {
        if (!packageEntityIds.Any())
        {
            return ImmutableDictionary<long, PackageBlueprintHistory[]>.Empty;
        }

        var startIndex = offset + 1;
        var endIndex = offset + limit;

        return (await _dbContext
                .PackageBlueprintHistory
                .FromSqlInterpolated($@"
WITH variables (package_entity_id) AS (SELECT UNNEST({packageEntityIds})),
blueprint_slices AS
(
    SELECT *
    FROM variables var
    INNER JOIN LATERAL (
        SELECT package_entity_id, package_blueprint_ids[{startIndex}:{endIndex}] AS blueprint_slice, cardinality(package_blueprint_ids) AS total_count
        FROM package_blueprint_aggregate_history
        WHERE package_entity_id = var.package_entity_id AND from_state_version <= {ledgerState.StateVersion}
        ORDER BY from_state_version DESC
        LIMIT 1
    ) pbah ON TRUE
)
SELECT pbh.*, bs.total_count
FROM blueprint_slices AS bs
INNER JOIN LATERAL UNNEST(blueprint_slice) WITH ORDINALITY AS blueprint_join(id, ordinality) ON TRUE
INNER JOIN package_blueprint_history pbh ON pbh.id = blueprint_join.id
ORDER BY blueprint_join.ordinality ASC;
")
                .AnnotateMetricName()
                .ToListAsync(token))
            .GroupBy(b => b.PackageEntityId)
            .ToDictionary(g => g.Key, g => g.ToArray());
    }

    private async Task<IDictionary<long, PackageCodeHistory[]>> GetPackageCodeHistory(long[] packageEntityIds, int offset, int limit, GatewayModel.LedgerState ledgerState, CancellationToken token)
    {
        if (!packageEntityIds.Any())
        {
            return ImmutableDictionary<long, PackageCodeHistory[]>.Empty;
        }

        var startIndex = offset + 1;
        var endIndex = offset + limit;

        return (await _dbContext
                .PackageCodeHistory
                .FromSqlInterpolated($@"
WITH variables (package_entity_id) AS (SELECT UNNEST({packageEntityIds})),
code_slices AS
(
    SELECT *
    FROM variables var
    INNER JOIN LATERAL (
        SELECT package_entity_id, package_code_ids[{startIndex}:{endIndex}] AS code_slice
        FROM package_code_aggregate_history
        WHERE package_entity_id = var.package_entity_id AND from_state_version <= {ledgerState.StateVersion}
        ORDER BY from_state_version DESC
        LIMIT 1
    ) pcah ON TRUE
)
SELECT pch.*
FROM code_slices AS cs
INNER JOIN LATERAL UNNEST(code_slice) WITH ORDINALITY AS code_join(id, ordinality) ON TRUE
INNER JOIN package_code_history pch ON pch.id = code_join.id
ORDER BY code_join.ordinality ASC;")
                .AnnotateMetricName()
                .ToListAsync(token))
            .GroupBy(b => b.PackageEntityId)
            .ToDictionary(g => g.Key, g => g.ToArray());
    }
}
