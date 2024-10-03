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
using RadixDlt.NetworkGateway.Abstractions;
using RadixDlt.NetworkGateway.Abstractions.Extensions;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using RadixDlt.NetworkGateway.PostgresIntegration.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using CoreModel = RadixDlt.CoreApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension;

internal class EntityStateProcessor
{
    private readonly ProcessorContext _context;
    private readonly ReferencedEntityDictionary _referencedEntities;

    private List<StateHistory> _toAdd = new();

    public EntityStateProcessor(ProcessorContext context, ReferencedEntityDictionary referencedEntities)
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
