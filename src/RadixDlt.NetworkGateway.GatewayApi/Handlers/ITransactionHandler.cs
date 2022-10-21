using RadixDlt.NetworkGateway.GatewayApiSdk.Model;
using System.Threading;
using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.GatewayApi.Handlers;

public interface ITransactionHandler
{
    Task<RecentTransactionsResponse> Recent(RecentTransactionsRequest request, CancellationToken token = default);

    Task<TransactionStatusResponse> Status(TransactionStatusRequest request, CancellationToken token = default);

    Task<TransactionDetailsResponse> Details(TransactionDetailsRequest request, CancellationToken token = default);

    Task<TransactionPreviewResponse> Preview(TransactionPreviewRequest request, CancellationToken token = default);

    Task<TransactionSubmitResponse> Submit(TransactionSubmitRequest request, CancellationToken token = default);
}
