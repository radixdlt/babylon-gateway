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

internal class LedgerTransactionMarkersProcessor : IProcessorBase, ISubstateUpsertProcessor, ISubstateDeleteProcessor, IDecodedEventProcessor, IEventProcessor, ITransactionProcessor
{
    private readonly ProcessorContext _context;
    private readonly IWriteHelper _writeHelper;
    private readonly List<LedgerTransactionMarker> _ledgerTransactionMarkersToAdd = new();
    private readonly ManifestProcessor _manifestProcessor;
    private readonly OriginLedgerTransactionMarkerProcessor _originLedgerTransactionMarkerProcessor;
    private readonly GlobalEventEmitterProcessor _globalEventEmitterProcessor;
    private readonly AffectedGlobalEntitiesProcessor _affectedGlobalEntitiesProcessor;
    private readonly EventLedgerTransactionMarkerProcessor _eventLedgerTransactionMarkerProcessor;

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
        _originLedgerTransactionMarkerProcessor = new OriginLedgerTransactionMarkerProcessor(context, referencedEntities, networkConfiguration);
        _globalEventEmitterProcessor = new GlobalEventEmitterProcessor(context, referencedEntities, networkConfiguration);
        _eventLedgerTransactionMarkerProcessor = new EventLedgerTransactionMarkerProcessor(context, referencedEntities, networkConfiguration);
    }

    public Task LoadDependenciesAsync()
    {
        return Task.CompletedTask;
    }

    public void VisitTransaction(CoreModel.CommittedTransaction transaction, long stateVersion)
    {
        _manifestProcessor.VisitTransaction(transaction, stateVersion);
        _originLedgerTransactionMarkerProcessor.VisitTransaction(transaction, stateVersion);
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
        _ledgerTransactionMarkersToAdd.AddRange(_originLedgerTransactionMarkerProcessor.CreateTransactionMarkers());
    }

    public async Task<int> SaveEntitiesAsync()
    {
        var rowsInserted = 0;

        rowsInserted += await CopyLedgerTransactionMarkers();

        return rowsInserted;
    }

    private Task<int> CopyLedgerTransactionMarkers() => _context.WriteHelper.Copy(
        _ledgerTransactionMarkersToAdd,
        "COPY ledger_transaction_markers (id, state_version, discriminator, event_type, entity_id, resource_entity_id, quantity, operation_type, origin_type, manifest_class, is_most_specific) FROM STDIN (FORMAT BINARY)",
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
                    break;
                case OriginLedgerTransactionMarker oltm:
                    await writer.WriteNullAsync(token);
                    await writer.WriteNullAsync(token);
                    await writer.WriteNullAsync(token);
                    await writer.WriteNullAsync(token);
                    await writer.WriteNullAsync(token);
                    await writer.WriteAsync(oltm.OriginType, "ledger_transaction_marker_origin_type", token);
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
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(e), e, null);
            }
        });
}
