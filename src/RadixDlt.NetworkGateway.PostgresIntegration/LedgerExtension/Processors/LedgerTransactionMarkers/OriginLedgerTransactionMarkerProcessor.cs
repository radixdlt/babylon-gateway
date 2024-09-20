using RadixDlt.NetworkGateway.Abstractions.Network;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using System.Collections.Generic;

namespace RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension.Processors.LedgerTransactionMarkers;

internal class OriginLedgerTransactionMarkerProcessor : ITransactionMarkerProcessor
{
    private readonly ProcessorContext _context;
    private readonly List<OriginLedgerTransactionMarker> _ledgerTransactionMarkersToAdd = new();

    public OriginLedgerTransactionMarkerProcessor(ProcessorContext context, ReferencedEntityDictionary _, NetworkConfiguration __)
    {
        _context = context;
    }

    public void VisitTransaction(CoreApiSdk.Model.CommittedTransaction committedTransaction, long stateVersion)
    {
        if (committedTransaction.Receipt.NextEpoch != null)
        {
            _ledgerTransactionMarkersToAdd.Add(
                new OriginLedgerTransactionMarker
                {
                    Id = _context.Sequences.LedgerTransactionMarkerSequence++,
                    StateVersion = stateVersion,
                    OriginType = LedgerTransactionMarkerOriginType.EpochChange,
                });
        }

        if (committedTransaction.LedgerTransaction is CoreApiSdk.Model.UserLedgerTransaction)
        {
            _ledgerTransactionMarkersToAdd.Add(
                new OriginLedgerTransactionMarker
                {
                    Id = _context.Sequences.LedgerTransactionMarkerSequence++,
                    StateVersion = stateVersion,
                    OriginType = LedgerTransactionMarkerOriginType.User,
                });
        }
    }

    public IEnumerable<LedgerTransactionMarker> CreateTransactionMarkers()
    {
        return _ledgerTransactionMarkersToAdd;
    }
}
