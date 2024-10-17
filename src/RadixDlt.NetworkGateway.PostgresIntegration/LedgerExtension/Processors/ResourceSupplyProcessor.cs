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
using RadixDlt.NetworkGateway.Abstractions.Numerics;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using RadixDlt.NetworkGateway.PostgresIntegration.Utils;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using ToolkitModel = RadixEngineToolkit;

namespace RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension;

internal class ResourceSupplyProcessor : IProcessorBase, IDecodedEventProcessor
{
    private readonly ProcessorContext _context;
    private readonly List<ResourceSupplyChange> _changes = new();
    private readonly Dictionary<long, ResourceEntitySupplyHistory> _mostRecent = new();
    private List<ResourceEntitySupplyHistory> _toAdd = new();

    public ResourceSupplyProcessor(ProcessorContext context)
    {
        _context = context;
    }

    public void VisitDecodedEvent(ToolkitModel.TypedNativeEvent decodedEvent, ReferencedEntity eventEmitterEntity, long stateVersion)
    {
        if (EventDecoder.TryGetFungibleResourceMintedEvent(decodedEvent, out var fungibleResourceMintedEvent))
        {
            var mintedAmount = TokenAmount.FromDecimalString(fungibleResourceMintedEvent.amount.AsStr());
            _changes.Add(new ResourceSupplyChange(eventEmitterEntity.DatabaseId, stateVersion, Minted: mintedAmount));
        }
        else if (EventDecoder.TryGetFungibleResourceBurnedEvent(decodedEvent, out var fungibleResourceBurnedEvent))
        {
            var burnedAmount = TokenAmount.FromDecimalString(fungibleResourceBurnedEvent.amount.AsStr());
            _changes.Add(new ResourceSupplyChange(eventEmitterEntity.DatabaseId, stateVersion, Burned: burnedAmount));
        }
        else if (EventDecoder.TryGetNonFungibleResourceMintedEvent(decodedEvent, out var nonFungibleResourceMintedEvent))
        {
            var mintedCount = TokenAmount.FromDecimalString(nonFungibleResourceMintedEvent.ids.Length.ToString());
            _changes.Add(new ResourceSupplyChange(eventEmitterEntity.DatabaseId, stateVersion, Minted: mintedCount));
        }
        else if (EventDecoder.TryGetNonFungibleResourceBurnedEvent(decodedEvent, out var nonFungibleResourceBurnedEvent))
        {
            var burnedCount = TokenAmount.FromDecimalString(nonFungibleResourceBurnedEvent.ids.Length.ToString());
            _changes.Add(new ResourceSupplyChange(eventEmitterEntity.DatabaseId, stateVersion, Burned: burnedCount));
        }
    }

    public async Task LoadDependenciesAsync()
    {
        _mostRecent.AddRange(await ExistingResourceEntitySupplyHistory());
    }

    public void ProcessChanges()
    {
        _toAdd = _changes
            .GroupBy(x => new { x.ResourceEntityId, x.StateVersion })
            .Select(
                group =>
                {
                    var previous = _mostRecent.GetOrAdd(
                        group.Key.ResourceEntityId,
                        _ => new ResourceEntitySupplyHistory { TotalSupply = TokenAmount.Zero, TotalMinted = TokenAmount.Zero, TotalBurned = TokenAmount.Zero });

                    var minted = group
                        .Where(x => x.Minted.HasValue)
                        .Select(x => x.Minted)
                        .Aggregate(TokenAmount.Zero, (sum, x) => sum + x!.Value);

                    var burned = group
                        .Where(x => x.Burned.HasValue)
                        .Select(x => x.Burned)
                        .Aggregate(TokenAmount.Zero, (sum, x) => sum + x!.Value);

                    var totalSupply = previous.TotalSupply + minted - burned;
                    var totalMinted = previous.TotalMinted + minted;
                    var totalBurned = previous.TotalBurned + burned;

                    previous.TotalSupply = totalSupply;
                    previous.TotalMinted = totalMinted;
                    previous.TotalBurned = totalBurned;

                    return new ResourceEntitySupplyHistory
                    {
                        Id = _context.Sequences.ResourceEntitySupplyHistorySequence++,
                        FromStateVersion = group.Key.StateVersion,
                        ResourceEntityId = group.Key.ResourceEntityId,
                        TotalSupply = totalSupply,
                        TotalMinted = totalMinted,
                        TotalBurned = totalBurned,
                    };
                })
            .ToList();
    }

    public async Task<int> SaveEntitiesAsync()
    {
        var rowsInserted = 0;

        rowsInserted += await CopyResourceSupplyHistory();

        return rowsInserted;
    }

    private Task<int> CopyResourceSupplyHistory() => _context.WriteHelper.Copy(
        _toAdd,
        "COPY resource_entity_supply_history (id, from_state_version, resource_entity_id, total_supply, total_minted, total_burned) FROM STDIN (FORMAT BINARY)",
        async (writer, e, token) =>
        {
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.ResourceEntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.TotalSupply.GetSubUnitsSafeForPostgres(), NpgsqlDbType.Numeric, token);
            await writer.WriteAsync(e.TotalMinted.GetSubUnitsSafeForPostgres(), NpgsqlDbType.Numeric, token);
            await writer.WriteAsync(e.TotalBurned.GetSubUnitsSafeForPostgres(), NpgsqlDbType.Numeric, token);
        });

    private async Task<IDictionary<long, ResourceEntitySupplyHistory>> ExistingResourceEntitySupplyHistory()
    {
        if (_changes.Count == 0)
        {
            return ImmutableDictionary<long, ResourceEntitySupplyHistory>.Empty;
        }

        var ids = _changes.Select(x => x.ResourceEntityId).ToHashSet();

        return await _context.ReadHelper.LoadDependencies<long, ResourceEntitySupplyHistory>(
            @$"
WITH variables (resource_entity_id) AS (
    SELECT UNNEST({ids})
)
SELECT rmesh.*
FROM variables
INNER JOIN LATERAL (
    SELECT *
    FROM resource_entity_supply_history
    WHERE resource_entity_id = variables.resource_entity_id
    ORDER BY from_state_version DESC
    LIMIT 1
) rmesh ON true;",
            e => e.ResourceEntityId);
    }
}
