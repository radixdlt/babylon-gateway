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

using Dapper;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension;

internal class SequencesHolder
{
    public long ComponentEntityStateHistorySequence { get; set; }

    public long EntitySequence { get; set; }

    public long EntityAccessRulesChainHistorySequence { get; set; }

    public long EntityMetadataHistorySequence { get; set; }

    public long EntityResourceAggregateHistorySequence { get; set; }

    public long EntityResourceHistorySequence { get; set; }

    public long FungibleResourceSupplyHistorySequence { get; set; }

    public long NonFungibleIdDataSequence { get; set; }

    public long NonFungibleIdMutableDataHistorySequence { get; set; }

    public long NonFungibleIdStoreHistorySequence { get; set; }

    public long NextComponentEntityStateHistory => ComponentEntityStateHistorySequence++;

    public long NextEntity => EntitySequence++;

    public long NextEntityAccessRulesChainHistory => EntityAccessRulesChainHistorySequence++;

    public long NextEntityMetadataHistory => EntityMetadataHistorySequence++;

    public long NextEntityResourceAggregateHistory => EntityResourceAggregateHistorySequence++;

    public long NextEntityResourceHistory => EntityResourceHistorySequence++;

    public long NextFungibleResourceSupplyHistory => FungibleResourceSupplyHistorySequence++;

    public long NextNonFungibleIdData => NonFungibleIdDataSequence++;

    public long NextNonFungibleIdMutableDataHistory => NonFungibleIdMutableDataHistorySequence++;

    public long NextNonFungibleIdStoreHistory => NonFungibleIdStoreHistorySequence++;

    public static async Task<SequencesHolder> Initialize(IDbConnection conn, CancellationToken token = default)
    {
        var cd = new CommandDefinition(
            commandText: @"
SELECT
    nextval('component_entity_state_history_id_seq') AS ComponentEntityStateHistorySequence,
    nextval('entities_id_seq') AS EntitySequence,
    nextval('entity_access_rules_chain_history_id_seq') AS EntityAccessRulesChainHistorySequence,
    nextval('entity_metadata_history_id_seq') AS EntityMetadataHistorySequence,
    nextval('entity_resource_aggregate_history_id_seq') AS EntityResourceAggregateHistorySequence,
    nextval('entity_resource_history_id_seq') AS EntityResourceHistorySequence,
    nextval('fungible_resource_supply_history_id_seq') AS FungibleResourceSupplyHistorySequence,
    nextval('non_fungible_id_data_id_seq') AS NonFungibleIdDataSequence,
    nextval('non_fungible_id_mutable_data_history_id_seq') AS NonFungibleIdMutableDataHistorySequence,
    nextval('non_fungible_id_store_history_id_seq') AS NonFungibleIdStoreHistorySequence",
            cancellationToken: token);

        return await conn.QueryFirstAsync<SequencesHolder>(cd);
    }

    public async Task Update(IDbConnection conn, CancellationToken token = default)
    {
        var cd = new CommandDefinition(
            commandText: @"
SELECT
    setval('component_entity_state_history_id_seq', @componentEntityStateHistorySequence),
    setval('entities_id_seq', @entitySequence),
    setval('entity_access_rules_chain_history_id_seq', @entityAccessRulesChainHistorySequence),
    setval('entity_metadata_history_id_seq', @entityMetadataHistorySequence),
    setval('entity_resource_aggregate_history_id_seq', @entityResourceAggregateHistorySequence),
    setval('entity_resource_history_id_seq', @entityResourceHistorySequence),
    setval('fungible_resource_supply_history_id_seq', @fungibleResourceSupplyHistorySequence),
    setval('non_fungible_id_data_id_seq', @nonFungibleIdDataSequence),
    setval('non_fungible_id_mutable_data_history_id_seq', @nonFungibleIdMutableDataHistorySequence),
    setval('non_fungible_id_store_history_id_seq', @nonFungibleIdStoreHistorySequence)",
            parameters: new
            {
                componentEntityStateHistorySequence = ComponentEntityStateHistorySequence,
                entitySequence = EntitySequence,
                entityAccessRulesChainHistorySequence = EntityAccessRulesChainHistorySequence,
                entityMetadataHistorySequence = EntityMetadataHistorySequence,
                entityResourceAggregateHistorySequence = EntityResourceAggregateHistorySequence,
                entityResourceHistorySequence = EntityResourceHistorySequence,
                fungibleResourceSupplyHistorySequence = FungibleResourceSupplyHistorySequence,
                nonFungibleIdDataSequence = NonFungibleIdDataSequence,
                nonFungibleIdMutableDataHistorySequence = NonFungibleIdMutableDataHistorySequence,
                nonFungibleIdStoreHistorySequence = NonFungibleIdStoreHistorySequence,
            },
            cancellationToken: token);

        await conn.ExecuteAsync(cd);
    }
}
