using RadixDlt.NetworkGateway.Abstractions.Extensions;
using RadixDlt.NetworkGateway.DataAggregator.NodeServices;
using RadixDlt.NetworkGateway.DataAggregator.NodeServices.ApiReaders;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CoreModel = RadixDlt.CoreApiSdk.Model;

namespace RadixDlt.NetworkGateway.DataAggregator.Services;

public interface INodeStatusProvider
{
    void UpdateNodeStatus(string nodeName, CoreModel.NetworkStatusResponse networkStatus);

    long GetHighestKnownStateVersion();
}

public sealed class NodeStatusProvider : INodeStatusProvider
{
    private readonly ConcurrentDictionary<string, long> _latestLedgerTipByNode = new();
    private readonly IEnumerable<ILedgerConfirmationServiceObserver> _observers;

    public NodeStatusProvider(IEnumerable<ILedgerConfirmationServiceObserver> observers)
    {
        _observers = observers;
    }

    public void UpdateNodeStatus(string nodeName, CoreModel.NetworkStatusResponse networkStatus)
    {
        var ledgerTipStateVersion = networkStatus.CurrentStateIdentifier.StateVersion;

        _observers.ForEach(x => x.PreSubmitNodeNetworkStatus(nodeName, ledgerTipStateVersion));
        _latestLedgerTipByNode[nodeName] = ledgerTipStateVersion;
    }

    public long GetHighestKnownStateVersion()
    {
        var ledgerTips = _latestLedgerTipByNode.Values.ToList();

        if (ledgerTips.Count == 0)
        {
            throw new InvalidNodeStateException("At least one ledger tip must have been submitted");
        }

        return ledgerTips.Max();
    }
}
