using RadixDlt.NetworkGateway.GatewayApi.Exceptions;
using RadixDlt.NetworkGateway.GatewayApi.Services;
using RadixDlt.NetworkGateway.GatewayApiSdk.Model;
using System.Threading;
using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.GatewayApi.Handlers;

internal class DefaultTransactionHandler : ITransactionHandler
{
    private readonly ILedgerStateQuerier _ledgerStateQuerier;
    private readonly ITransactionQuerier _transactionQuerier;
    private readonly IPreviewService _previewService;
    private readonly ISubmissionService _submissionService;

    public DefaultTransactionHandler(
        ILedgerStateQuerier ledgerStateQuerier,
        ITransactionQuerier transactionQuerier,
        IPreviewService previewService,
        ISubmissionService submissionService)
    {
        _ledgerStateQuerier = ledgerStateQuerier;
        _transactionQuerier = transactionQuerier;
        _previewService = previewService;
        _submissionService = submissionService;
    }

    public async Task<RecentTransactionsResponse> Recent(RecentTransactionsRequest request, CancellationToken token = default)
    {
        var atLedgerState = await _ledgerStateQuerier.GetValidLedgerStateForReadRequest(request.AtStateIdentifier, token);
        var fromLedgerState = await _ledgerStateQuerier.GetValidLedgerStateForReadForwardRequest(request.FromStateIdentifier, token);

        var transactionsPageRequest = new RecentTransactionPageRequest(
            Cursor: CommittedTransactionPaginationCursor.FromCursorString(request.Cursor),
            PageSize: request.Limit ?? 10
        );

        var results = await _transactionQuerier.GetRecentUserTransactions(transactionsPageRequest, atLedgerState, fromLedgerState, token);

        // NB - We don't return a total here as we don't have an index on user transactions
        return new RecentTransactionsResponse(
            atLedgerState,
            nextCursor: results.NextPageCursor?.ToCursorString(),
            items: results.Transactions
        );
    }

    public async Task<TransactionStatusResponse> Status(TransactionStatusRequest request, CancellationToken token = default)
    {
        var ledgerState = await _ledgerStateQuerier.GetValidLedgerStateForReadRequest(request.AtStateIdentifier, token);
        var committedTransaction = await _transactionQuerier.LookupCommittedTransaction(request.TransactionIdentifier, ledgerState, false, token);

        if (committedTransaction != null)
        {
            return new TransactionStatusResponse(ledgerState, committedTransaction.Info);
        }

        var pendingTransaction = await _transactionQuerier.LookupPendingTransaction(request.TransactionIdentifier, token);

        if (pendingTransaction != null)
        {
            return new TransactionStatusResponse(ledgerState, pendingTransaction);
        }

        throw new TransactionNotFoundException(request.TransactionIdentifier);
    }

    public async Task<TransactionDetailsResponse> Details(TransactionDetailsRequest request, CancellationToken token = default)
    {
        var ledgerState = await _ledgerStateQuerier.GetValidLedgerStateForReadRequest(request.AtStateIdentifier, token);
        var committedTransaction = await _transactionQuerier.LookupCommittedTransaction(request.TransactionIdentifier, ledgerState, true, token);

        if (committedTransaction != null)
        {
            return new TransactionDetailsResponse(ledgerState, committedTransaction.Info, committedTransaction.Details);
        }

        var pendingTransaction = await _transactionQuerier.LookupPendingTransaction(request.TransactionIdentifier, token);

        if (pendingTransaction != null)
        {
            return new TransactionDetailsResponse(ledgerState, pendingTransaction);
        }

        throw new TransactionNotFoundException(request.TransactionIdentifier);
    }

    public async Task<TransactionPreviewResponse> Preview(TransactionPreviewRequest request, CancellationToken token = default)
    {
        return await _previewService.HandlePreviewRequest(request, token);
    }

    public async Task<TransactionSubmitResponse> Submit(TransactionSubmitRequest request, CancellationToken token = default)
    {
        return await _submissionService.HandleSubmitRequest(request, token);
    }
}
