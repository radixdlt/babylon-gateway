using NpgsqlTypes;
using RadixDlt.NetworkGateway.Abstractions;
using RadixDlt.NetworkGateway.Abstractions.Extensions;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using CoreModel = RadixDlt.CoreApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension;

internal class Dumpyard_EntityState
{
    private readonly Dumpyard_Context _context;
    private readonly ReferencedEntityDictionary _referencedEntities;

    private List<StateHistory> _toAdd = new();

    public Dumpyard_EntityState(Dumpyard_Context context, ReferencedEntityDictionary referencedEntities)
    {
        _context = context;
        _referencedEntities = referencedEntities;
    }

    public void VisitUpsert(CoreModel.IUpsertedSubstate substate, ReferencedEntity referencedEntity, long stateVersion)
    {
        var substateData = substate.Value.SubstateData;

        if (substateData is CoreModel.GenericScryptoComponentFieldStateSubstate componentState)
        {
            if (substate.SystemStructure is not CoreModel.ObjectFieldStructure objectFieldStructure)
            {
                throw new UnreachableException($"Generic Scrypto components are expected to have ObjectFieldStructure. Got: {substate.SystemStructure.GetType()}");
            }

            var schemaDetails = objectFieldStructure.ValueSchema.GetSchemaDetails();

            _toAdd.Add(new SborStateHistory
            {
                Id = _context.Sequences.StateHistorySequence++,
                FromStateVersion = stateVersion,
                EntityId = referencedEntity.DatabaseId,
                SborState = componentState.Value.DataStruct.StructData.GetDataBytes(),
                SchemaHash = schemaDetails.SchemaHash.ConvertFromHex(),
                SborTypeKind = schemaDetails.SborTypeKind.ToModel(),
                TypeIndex = schemaDetails.TypeIndex,
                SchemaDefiningEntityId = _referencedEntities.Get((EntityAddress)schemaDetails.SchemaDefiningEntityAddress).DatabaseId,
            });
        }

        if (substateData is CoreModel.ValidatorFieldStateSubstate validator)
        {
            _toAdd.Add(new JsonStateHistory
            {
                Id = _context.Sequences.StateHistorySequence++,
                FromStateVersion = stateVersion,
                EntityId = referencedEntity.DatabaseId,
                JsonState = validator.Value.ToJson(),
            });
        }

        if (substateData is CoreModel.AccountFieldStateSubstate accountFieldState)
        {
            _toAdd.Add(new JsonStateHistory
            {
                Id = _context.Sequences.StateHistorySequence++,
                FromStateVersion = stateVersion,
                EntityId = referencedEntity.DatabaseId,
                JsonState = accountFieldState.Value.ToJson(),
            });
        }

        if (substateData is CoreModel.AccessControllerFieldStateSubstate accessControllerFieldState)
        {
            _toAdd.Add(new JsonStateHistory
            {
                Id = _context.Sequences.StateHistorySequence++,
                FromStateVersion = stateVersion,
                EntityId = referencedEntity.DatabaseId,
                JsonState = accessControllerFieldState.Value.ToJson(),
            });
        }

        if (substateData is CoreModel.OneResourcePoolFieldStateSubstate oneResourcePoolFieldStateSubstate)
        {
            _toAdd.Add(new JsonStateHistory
            {
                Id = _context.Sequences.StateHistorySequence++,
                FromStateVersion = stateVersion,
                EntityId = referencedEntity.DatabaseId,
                JsonState = oneResourcePoolFieldStateSubstate.Value.ToJson(),
            });
        }

        if (substateData is CoreModel.TwoResourcePoolFieldStateSubstate twoResourcePoolFieldStateSubstate)
        {
            _toAdd.Add(new JsonStateHistory
            {
                Id = _context.Sequences.StateHistorySequence++,
                FromStateVersion = stateVersion,
                EntityId = referencedEntity.DatabaseId,
                JsonState = twoResourcePoolFieldStateSubstate.Value.ToJson(),
            });
        }

        if (substateData is CoreModel.MultiResourcePoolFieldStateSubstate multiResourcePoolFieldStateSubstate)
        {
            _toAdd.Add(new JsonStateHistory
            {
                Id = _context.Sequences.StateHistorySequence++,
                FromStateVersion = stateVersion,
                EntityId = referencedEntity.DatabaseId,
                JsonState = multiResourcePoolFieldStateSubstate.Value.ToJson(),
            });
        }
    }

    public async Task<int> SaveEntities()
    {
        var rowsInserted = 0;

        rowsInserted += await CopyStateHistory();

        return rowsInserted;
    }

    private Task<int> CopyStateHistory() => _context.WriteHelper.Copy(
        _toAdd,
        "COPY state_history (id, from_state_version, entity_id, discriminator, json_state, sbor_state, type_index, schema_hash, sbor_type_kind, schema_defining_entity_id) FROM STDIN (FORMAT BINARY)",
        async (writer, e, token) =>
        {
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.EntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(_context.WriteHelper.GetDiscriminator<StateType>(e.GetType()), "state_type", token);

            switch (e)
            {
                case JsonStateHistory jsonStateHistory:
                    await writer.WriteAsync(jsonStateHistory.JsonState, NpgsqlDbType.Jsonb, token);
                    await writer.WriteNullAsync(token);
                    await writer.WriteNullAsync(token);
                    await writer.WriteNullAsync(token);
                    await writer.WriteNullAsync(token);
                    await writer.WriteNullAsync(token);
                    break;
                case SborStateHistory sborStateHistory:
                    await writer.WriteNullAsync(token);
                    await writer.WriteAsync(sborStateHistory.SborState, NpgsqlDbType.Bytea, token);
                    await writer.WriteAsync(sborStateHistory.TypeIndex, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(sborStateHistory.SchemaHash, NpgsqlDbType.Bytea, token);
                    await writer.WriteAsync(sborStateHistory.SborTypeKind, "sbor_type_kind", token);
                    await writer.WriteAsync(sborStateHistory.SchemaDefiningEntityId, NpgsqlDbType.Bigint, token);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(e), e, null);
            }
        });
}
