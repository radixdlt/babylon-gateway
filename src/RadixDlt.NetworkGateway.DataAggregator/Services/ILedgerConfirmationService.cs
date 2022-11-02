using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CoreModel = RadixDlt.CoreApiSdk.Model;

namespace RadixDlt.NetworkGateway.DataAggregator.Services;

public interface ILedgerConfirmationService
{
    // This method is to be called from the global LedgerExtensionWorker
    Task HandleLedgerExtensionIfQuorum(CancellationToken token);

    // Below are to be called from the node transaction log workers - to communicate with the LedgerConfirmationService
    void SubmitNodeNetworkStatus(string nodeName, long ledgerTipStateVersion, byte[] ledgerTipAccumulator, long targetStateVersion);

    void SubmitTransactionsFromNode(string nodeName, List<CoreModel.CommittedTransaction> transactions);

    TransactionsRequested? GetWhichTransactionsAreRequestedFromNode(string nodeName);
}

public sealed record TransactionsRequested(long StateVersionInclusiveLowerBound, long StateVersionInclusiveUpperBound);
