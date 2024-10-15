// <copyright file="EpochChangeLedgerTransactionMarkerProcessor.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using RadixDlt.NetworkGateway.Abstractions.Network;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using System.Collections.Generic;

namespace RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension.Processors.LedgerTransactionMarkers;

internal class EpochChangeLedgerTransactionMarkerProcessor : ITransactionMarkerProcessor
{
    private readonly ProcessorContext _context;
    private readonly List<EpochChangeLedgerTransactionMarker> _ledgerTransactionMarkersToAdd = new();

    public EpochChangeLedgerTransactionMarkerProcessor(ProcessorContext context)
    {
        _context = context;
    }

    public void VisitTransaction(CoreApiSdk.Model.CommittedTransaction committedTransaction, long stateVersion)
    {
        if (committedTransaction.Receipt.NextEpoch != null)
        {
            _ledgerTransactionMarkersToAdd.Add(
                new EpochChangeLedgerTransactionMarker
                {
                    Id = _context.Sequences.LedgerTransactionMarkerSequence++,
                    StateVersion = stateVersion,
                    EpochChange = true,
                });
        }
    }

    public IEnumerable<LedgerTransactionMarker> CreateTransactionMarkers()
    {
        return _ledgerTransactionMarkersToAdd;
    }
}
