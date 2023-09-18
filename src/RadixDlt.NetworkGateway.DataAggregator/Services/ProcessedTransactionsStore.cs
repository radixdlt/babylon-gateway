using RadixDlt.NetworkGateway.Abstractions.Extensions;
using RadixDlt.NetworkGateway.DataAggregator.Monitoring;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.DataAggregator.Services;

public interface ITopOfLedgerCache
{
    TransactionSummary GetLastCommittedTransactionSummary();

    long GetLastCommittedStateVersion();

    Task<TransactionSummary> Refresh(CancellationToken token);

    void Update(TransactionSummary transactionSummary);
}

public class TopOfLedgerCache : ITopOfLedgerCache
{
    private readonly ISystemStatusService _systemStatusService;
    private readonly ITopOfLedgerProvider _topOfLedgerProvider;
    private readonly IEnumerable<ILedgerConfirmationServiceObserver> _observers;

    private TransactionSummary? _knownTopOfCommittedLedger;

    public TopOfLedgerCache(ISystemStatusService systemStatusService, IEnumerable<ILedgerConfirmationServiceObserver> observers, ITopOfLedgerProvider topOfLedgerProvider)
    {
        _systemStatusService = systemStatusService;
        _observers = observers;
        _topOfLedgerProvider = topOfLedgerProvider;
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

    public async Task<TransactionSummary> Refresh(CancellationToken token)
    {
        var topOfLedger = await _topOfLedgerProvider.GetTopOfLedger(token);
        Update(topOfLedger);
        return topOfLedger;
    }

    public void Update(TransactionSummary transactionSummary)
    {
        _knownTopOfCommittedLedger = transactionSummary;

        _observers.ForEach(x => x.RecordTopOfDbLedger(_knownTopOfCommittedLedger.StateVersion, _knownTopOfCommittedLedger.RoundTimestamp));

        _systemStatusService.SetTopOfDbLedgerNormalizedRoundTimestamp(_knownTopOfCommittedLedger.NormalizedRoundTimestamp);
    }
}
