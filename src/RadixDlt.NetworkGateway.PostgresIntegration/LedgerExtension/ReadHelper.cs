using Dapper;
using Npgsql;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension;

internal class ReadHelper
{
    private readonly NpgsqlConnection _connection;

    public ReadHelper(NpgsqlConnection connection)
    {
        _connection = connection;
    }

    public async Task<SequencesHolder> LoadSequences(CancellationToken token)
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
    nextval('resource_manager_entity_supply_history_id_seq') AS ResourceManagerEntitySupplyHistorySequence,
    nextval('non_fungible_id_data_id_seq') AS NonFungibleIdDataSequence,
    nextval('non_fungible_id_mutable_data_history_id_seq') AS NonFungibleIdMutableDataHistorySequence,
    nextval('non_fungible_id_store_history_id_seq') AS NonFungibleIdStoreHistorySequence",
            cancellationToken: token);

        return await _connection.QueryFirstAsync<SequencesHolder>(cd);
    }
}
