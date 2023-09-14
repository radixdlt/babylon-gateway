using RadixDlt.NetworkGateway.Abstractions.Extensions;
using RadixDlt.NetworkGateway.DataAggregator.Monitoring;
using System.Collections.Generic;
using System.Diagnostics;

namespace RadixDlt.NetworkGateway.DataAggregator.Services;

public interface IProcessedTransactionsStore
{
    public bool IsInitialized();

    public TransactionSummary GetLastCommittedTransactionSummary();

    public long GetLastCommittedStateVersion();

    public void Update(TransactionSummary topOfLedger);
}

public class ProcessedTransactionsStore : IProcessedTransactionsStore
{
    private readonly ISystemStatusService _systemStatusService;
    private readonly IEnumerable<ILedgerConfirmationServiceObserver> _observers;

    private TransactionSummary? _knownTopOfCommittedLedger;

    public ProcessedTransactionsStore(ISystemStatusService systemStatusService, IEnumerable<ILedgerConfirmationServiceObserver> observers)
    {
        _systemStatusService = systemStatusService;
        _observers = observers;
    }

    public bool IsInitialized()
    {
        return _knownTopOfCommittedLedger != null;
    }

    public TransactionSummary GetLastCommittedTransactionSummary()
    {
        if (_knownTopOfCommittedLedger == null)
        {
            throw new UnreachableException("Unexpected situation. Last committed state version is null");
        }

        return _knownTopOfCommittedLedger;
    }

    public long GetLastCommittedStateVersion()
    {
        if (_knownTopOfCommittedLedger == null)
        {
            throw new UnreachableException("Unexpected situation. Last committed state version is null");
        }

        return _knownTopOfCommittedLedger.StateVersion;
    }

    public void Update(TransactionSummary topOfLedger)
    {
        _knownTopOfCommittedLedger = topOfLedger;

        _observers.ForEach(x => x.RecordTopOfDbLedger(topOfLedger.StateVersion, topOfLedger.RoundTimestamp));

        _systemStatusService.SetTopOfDbLedgerNormalizedRoundTimestamp(topOfLedger.NormalizedRoundTimestamp);
    }
}
