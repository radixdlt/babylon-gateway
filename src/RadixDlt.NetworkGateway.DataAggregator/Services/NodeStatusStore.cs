using RadixDlt.NetworkGateway.Abstractions.Extensions;
using RadixDlt.NetworkGateway.DataAggregator.NodeServices;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace RadixDlt.NetworkGateway.DataAggregator.Services;

public interface INodeStatusStore
{
    void StoreNodeStatus(string nodeName, long ledgerTipStateVersion);

    long GetHighestKnownStateVersion();
}

public sealed class NodeStatusStore : INodeStatusStore
{
    private readonly ConcurrentDictionary<string, long> _latestLedgerTipByNode = new();
    private readonly IEnumerable<ILedgerConfirmationServiceObserver> _observers;

    public NodeStatusStore(IEnumerable<ILedgerConfirmationServiceObserver> observers)
    {
        _observers = observers;
    }

    public void StoreNodeStatus(string nodeName, long ledgerTipStateVersion)
    {
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
