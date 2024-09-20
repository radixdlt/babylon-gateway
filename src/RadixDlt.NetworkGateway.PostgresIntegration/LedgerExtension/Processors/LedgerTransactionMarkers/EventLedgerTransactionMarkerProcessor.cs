using RadixDlt.NetworkGateway.Abstractions.Network;
using RadixDlt.NetworkGateway.Abstractions.Numerics;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using RadixDlt.NetworkGateway.PostgresIntegration.Utils;
using RadixEngineToolkit;
using System.Collections.Generic;

namespace RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension.Processors.LedgerTransactionMarkers;

internal class EventLedgerTransactionMarkerProcessor : ITransactionMarkerProcessor, IDecodedEventProcessor
{
    private readonly ProcessorContext _context;
    private readonly List<EventLedgerTransactionMarker> _ledgerTransactionMarkersToAdd = new();

    public EventLedgerTransactionMarkerProcessor(ProcessorContext context, ReferencedEntityDictionary _, NetworkConfiguration __)
    {
        _context = context;
    }

    public IEnumerable<LedgerTransactionMarker> CreateTransactionMarkers()
    {
        return _ledgerTransactionMarkersToAdd;
    }

    public void VisitDecodedEvent(TypedNativeEvent decodedEvent, ReferencedEntity eventEmitterEntity, long stateVersion)
    {
        if (EventDecoder.TryGetFungibleVaultWithdrawalEvent(decodedEvent, out var fungibleVaultWithdrawalEvent))
        {
            _ledgerTransactionMarkersToAdd.Add(
                new EventLedgerTransactionMarker
                {
                    Id = _context.Sequences.LedgerTransactionMarkerSequence++,
                    StateVersion = stateVersion,
                    EventType = LedgerTransactionMarkerEventType.Withdrawal,
                    EntityId = eventEmitterEntity.DatabaseGlobalAncestorId,
                    ResourceEntityId = eventEmitterEntity.GetDatabaseEntity<InternalFungibleVaultEntity>().GetResourceEntityId(),
                    Quantity = TokenAmount.FromDecimalString(fungibleVaultWithdrawalEvent.AsStr()),
                });
        }
        else if (EventDecoder.TryGetFungibleVaultDepositEvent(decodedEvent, out var fungibleVaultDepositEvent))
        {
            _ledgerTransactionMarkersToAdd.Add(
                new EventLedgerTransactionMarker
                {
                    Id = _context.Sequences.LedgerTransactionMarkerSequence++,
                    StateVersion = stateVersion,
                    EventType = LedgerTransactionMarkerEventType.Deposit,
                    EntityId = eventEmitterEntity.DatabaseGlobalAncestorId,
                    ResourceEntityId = eventEmitterEntity.GetDatabaseEntity<InternalFungibleVaultEntity>().GetResourceEntityId(),
                    Quantity = TokenAmount.FromDecimalString(fungibleVaultDepositEvent.AsStr()),
                });
        }
        else if (EventDecoder.TryGetNonFungibleVaultWithdrawalEvent(decodedEvent, out var nonFungibleVaultWithdrawalEvent))
        {
            _ledgerTransactionMarkersToAdd.Add(
                new EventLedgerTransactionMarker
                {
                    Id = _context.Sequences.LedgerTransactionMarkerSequence++,
                    StateVersion = stateVersion,
                    EventType = LedgerTransactionMarkerEventType.Withdrawal,
                    EntityId = eventEmitterEntity.DatabaseGlobalAncestorId,
                    ResourceEntityId = eventEmitterEntity.GetDatabaseEntity<InternalNonFungibleVaultEntity>().GetResourceEntityId(),
                    Quantity = TokenAmount.FromDecimalString(nonFungibleVaultWithdrawalEvent.Length.ToString()),
                });
        }
        else if (EventDecoder.TryGetNonFungibleVaultDepositEvent(decodedEvent, out var nonFungibleVaultDepositEvent))
        {
            _ledgerTransactionMarkersToAdd.Add(
                new EventLedgerTransactionMarker
                {
                    Id = _context.Sequences.LedgerTransactionMarkerSequence++,
                    StateVersion = stateVersion,
                    EventType = LedgerTransactionMarkerEventType.Deposit,
                    EntityId = eventEmitterEntity.DatabaseGlobalAncestorId,
                    ResourceEntityId = eventEmitterEntity.GetDatabaseEntity<InternalNonFungibleVaultEntity>().GetResourceEntityId(),
                    Quantity = TokenAmount.FromDecimalString(nonFungibleVaultDepositEvent.Length.ToString()),
                });
        }
    }
}
