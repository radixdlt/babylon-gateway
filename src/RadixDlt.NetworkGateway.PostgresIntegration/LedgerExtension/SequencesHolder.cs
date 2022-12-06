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
