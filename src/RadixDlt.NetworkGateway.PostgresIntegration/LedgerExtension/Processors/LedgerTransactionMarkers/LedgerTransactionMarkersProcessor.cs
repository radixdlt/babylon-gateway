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
using RadixDlt.NetworkGateway.Abstractions.Model;
using RadixDlt.NetworkGateway.Abstractions.Network;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using RadixDlt.NetworkGateway.PostgresIntegration.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CoreModel = RadixDlt.CoreApiSdk.Model;
using ToolkitModel = RadixEngineToolkit;

namespace RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension.Processors.LedgerTransactionMarkers;

internal class LedgerTransactionMarkersProcessor : IProcessorBase, ISubstateUpsertProcessor, ISubstateDeleteProcessor, IDecodedEventProcessor, IEventProcessor, ITransactionProcessor, ITransactionScanProcessor
{
    private readonly ProcessorContext _context;
    private readonly IWriteHelper _writeHelper;
    private readonly List<LedgerTransactionMarker> _ledgerTransactionMarkersToAdd = new();
    private readonly ManifestProcessor _manifestProcessor;
    private readonly TransactionTypeLedgerTransactionMarkerProcessor _transactionTypeLedgerTransactionMarkerProcessor;
    private readonly GlobalEventEmitterProcessor _globalEventEmitterProcessor;
    private readonly AffectedGlobalEntitiesProcessor _affectedGlobalEntitiesProcessor;
    private readonly EventLedgerTransactionMarkerProcessor _eventLedgerTransactionMarkerProcessor;
    private readonly EpochChangeLedgerTransactionMarkerProcessor _epochChangeLedgerTransactionMarkerProcessor;

    public LedgerTransactionMarkersProcessor(
        ManifestProcessor manifestProcessor,
        AffectedGlobalEntitiesProcessor affectedGlobalEntitiesProcessor,
        ProcessorContext context,
        ReferencedEntityDictionary referencedEntities,
        IWriteHelper writeHelper,
        NetworkConfiguration networkConfiguration)
    {
        _context = context;
        _writeHelper = writeHelper;
        _manifestProcessor = manifestProcessor;
        _affectedGlobalEntitiesProcessor = affectedGlobalEntitiesProcessor;
        _transactionTypeLedgerTransactionMarkerProcessor = new TransactionTypeLedgerTransactionMarkerProcessor(context);
        _globalEventEmitterProcessor = new GlobalEventEmitterProcessor(context, referencedEntities, networkConfiguration);
        _eventLedgerTransactionMarkerProcessor = new EventLedgerTransactionMarkerProcessor(context);
        _epochChangeLedgerTransactionMarkerProcessor = new EpochChangeLedgerTransactionMarkerProcessor(context);
    }

    public Task LoadDependenciesAsync()
    {
        return Task.CompletedTask;
    }

    public void OnTransactionScan(CoreModel.CommittedTransaction transaction, long stateVersion)
    {
        _manifestProcessor.OnTransactionScan(transaction, stateVersion);
    }

    public void VisitTransaction(CoreModel.CommittedTransaction transaction, long stateVersion)
    {
        _transactionTypeLedgerTransactionMarkerProcessor.VisitTransaction(transaction, stateVersion);
        _epochChangeLedgerTransactionMarkerProcessor.VisitTransaction(transaction, stateVersion);
    }

    public void VisitUpsert(CoreModel.IUpsertedSubstate substate, ReferencedEntity referencedEntity, long stateVersion)
    {
        _affectedGlobalEntitiesProcessor.VisitUpsert(substate, referencedEntity, stateVersion);
    }

    public void VisitDelete(CoreModel.SubstateId substateId, ReferencedEntity referencedEntity, long stateVersion)
    {
        _affectedGlobalEntitiesProcessor.VisitDelete(substateId, referencedEntity, stateVersion);
    }

    public void VisitDecodedEvent(ToolkitModel.TypedNativeEvent decodedEvent, ReferencedEntity eventEmitterEntity, long stateVersion)
    {
        _eventLedgerTransactionMarkerProcessor.VisitDecodedEvent(decodedEvent, eventEmitterEntity, stateVersion);
    }

    public void VisitEvent(CoreModel.Event @event, long stateVersion)
    {
        _globalEventEmitterProcessor.VisitEvent(@event, stateVersion);
    }

    public void ProcessChanges()
    {
        _ledgerTransactionMarkersToAdd.AddRange(_globalEventEmitterProcessor.CreateTransactionMarkers());
        _ledgerTransactionMarkersToAdd.AddRange(_affectedGlobalEntitiesProcessor.CreateTransactionMarkers());
        _ledgerTransactionMarkersToAdd.AddRange(_eventLedgerTransactionMarkerProcessor.CreateTransactionMarkers());
        _ledgerTransactionMarkersToAdd.AddRange(_manifestProcessor.CreateTransactionMarkers());
        _ledgerTransactionMarkersToAdd.AddRange(_transactionTypeLedgerTransactionMarkerProcessor.CreateTransactionMarkers());
        _ledgerTransactionMarkersToAdd.AddRange(_epochChangeLedgerTransactionMarkerProcessor.CreateTransactionMarkers());
    }

    public async Task<int> SaveEntitiesAsync()
    {
        var rowsInserted = 0;

        rowsInserted += await CopyLedgerTransactionMarkers();

        return rowsInserted;
    }

    private Task<int> CopyLedgerTransactionMarkers() => _context.WriteHelper.Copy(
        _ledgerTransactionMarkersToAdd,
        "COPY ledger_transaction_markers (id, state_version, discriminator, event_type, entity_id, resource_entity_id, quantity, operation_type, transaction_type, manifest_class, is_most_specific, epoch_change) FROM STDIN (FORMAT BINARY)",
        async (writer, e, token) =>
        {
            var discriminator = _writeHelper.GetDiscriminator<LedgerTransactionMarkerType>(e.GetType());

            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.StateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(discriminator, "ledger_transaction_marker_type", token);

            switch (e)
            {
                case EventLedgerTransactionMarker eltm:
                    await writer.WriteAsync(eltm.EventType, "ledger_transaction_marker_event_type", token);
                    await writer.WriteAsync(eltm.EntityId, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(eltm.ResourceEntityId, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(eltm.Quantity.GetSubUnitsSafeForPostgres(), NpgsqlDbType.Numeric, token);
                    await writer.WriteNullAsync(token);
                    await writer.WriteNullAsync(token);
                    await writer.WriteNullAsync(token);
                    await writer.WriteNullAsync(token);
                    await writer.WriteNullAsync(token);
                    break;
                case ManifestAddressLedgerTransactionMarker maltm:
                    await writer.WriteNullAsync(token);
                    await writer.WriteAsync(maltm.EntityId, NpgsqlDbType.Bigint, token);
                    await writer.WriteNullAsync(token);
                    await writer.WriteNullAsync(token);
                    await writer.WriteAsync(maltm.OperationType, "ledger_transaction_marker_operation_type", token);
                    await writer.WriteNullAsync(token);
                    await writer.WriteNullAsync(token);
                    await writer.WriteNullAsync(token);
                    await writer.WriteNullAsync(token);
                    break;
                case TransactionTypeLedgerTransactionMarker oltm:
                    await writer.WriteNullAsync(token);
                    await writer.WriteNullAsync(token);
                    await writer.WriteNullAsync(token);
                    await writer.WriteNullAsync(token);
                    await writer.WriteNullAsync(token);
                    await writer.WriteAsync(oltm.TransactionType, "ledger_transaction_marker_transaction_type", token);
                    await writer.WriteNullAsync(token);
                    await writer.WriteNullAsync(token);
                    await writer.WriteNullAsync(token);
                    break;
                case AffectedGlobalEntityTransactionMarker oltm:
                    await writer.WriteNullAsync(token);
                    await writer.WriteAsync(oltm.EntityId, NpgsqlDbType.Bigint, token);
                    await writer.WriteNullAsync(token);
                    await writer.WriteNullAsync(token);
                    await writer.WriteNullAsync(token);
                    await writer.WriteNullAsync(token);
                    await writer.WriteNullAsync(token);
                    await writer.WriteNullAsync(token);
                    await writer.WriteNullAsync(token);
                    break;
                case EventGlobalEmitterTransactionMarker egetm:
                    await writer.WriteNullAsync(token);
                    await writer.WriteAsync(egetm.EntityId, NpgsqlDbType.Bigint, token);
                    await writer.WriteNullAsync(token);
                    await writer.WriteNullAsync(token);
                    await writer.WriteNullAsync(token);
                    await writer.WriteNullAsync(token);
                    await writer.WriteNullAsync(token);
                    await writer.WriteNullAsync(token);
                    await writer.WriteNullAsync(token);
                    break;
                case ManifestClassMarker ttm:
                    await writer.WriteNullAsync(token);
                    await writer.WriteNullAsync(token);
                    await writer.WriteNullAsync(token);
                    await writer.WriteNullAsync(token);
                    await writer.WriteNullAsync(token);
                    await writer.WriteNullAsync(token);
                    await writer.WriteAsync(ttm.ManifestClass, "ledger_transaction_manifest_class", token);
                    await writer.WriteAsync(ttm.IsMostSpecific, NpgsqlDbType.Boolean, token);
                    await writer.WriteNullAsync(token);
                    break;
                case EpochChangeLedgerTransactionMarker ectm:
                    await writer.WriteNullAsync(token);
                    await writer.WriteNullAsync(token);
                    await writer.WriteNullAsync(token);
                    await writer.WriteNullAsync(token);
                    await writer.WriteNullAsync(token);
                    await writer.WriteNullAsync(token);
                    await writer.WriteNullAsync(token);
                    await writer.WriteNullAsync(token);
                    await writer.WriteAsync(ectm.EpochChange, token);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(e), e, null);
            }
        });
}
