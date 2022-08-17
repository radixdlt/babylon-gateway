using RadixDlt.CoreApiSdk.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.DataAggregator.Workers.NodeWorkers;

public interface INodeTransactionLogWorkerObserver
{
    ValueTask DoWorkFailed(string nodeName, Exception exception);

    ValueTask TransactionsFetched(string nodeName, List<CommittedTransaction> transactions, long fetchTransactionsMs);
}
