using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RadixDlt.NetworkGateway.Frontend.Configuration;
using RadixDlt.NetworkGateway.Frontend.Exceptions;
using RadixDlt.NetworkGateway.Frontend.Services;
using RadixDlt.NetworkGateway.FrontendSdk.Model;

namespace RadixDlt.NetworkGateway.Frontend.Endpoints;

[ApiController]
[Route("transaction")]
[TypeFilter(typeof(ExceptionFilter))]
[TypeFilter(typeof(InvalidModelStateFilter))]
public class TransactionController
{
    private readonly IValidations _validations;
    private readonly ILedgerStateQuerier _ledgerStateQuerier;
    private readonly ITransactionQuerier _transactionQuerier;
    private readonly IConstructionAndSubmissionService _constructionAndSubmissionService;
    private readonly EndpointOptions _endpointOptions;

    public TransactionController(
        IValidations validations,
        ILedgerStateQuerier ledgerStateQuerier,
        ITransactionQuerier transactionQuerier,
        IConstructionAndSubmissionService constructionAndSubmissionService,
        IOptionsSnapshot<EndpointOptions> endpointOptionsSnapshot
    )
    {
        _validations = validations;
        _ledgerStateQuerier = ledgerStateQuerier;
        _transactionQuerier = transactionQuerier;
        _constructionAndSubmissionService = constructionAndSubmissionService;
        _endpointOptions = endpointOptionsSnapshot.Value;
    }

    [HttpPost("recent")]
    public async Task<RecentTransactionsResponse> Recent(RecentTransactionsRequest request)
    {
        var ledgerState = await _ledgerStateQuerier.GetValidLedgerStateForReadRequest(request.AtStateIdentifier);

        var unvalidatedLimit = request.Limit is default(int) ? 10 : request.Limit;

        var transactionsPageRequest = new RecentTransactionPageRequest(
            Cursor: CommittedTransactionPaginationCursor.FromCursorString(request.Cursor),
            PageSize: _validations.ExtractValidIntInBoundInclusive(
                "Page size",
                unvalidatedLimit,
                1,
                _endpointOptions.MaxPageSize
            )
        );

        var results = await _transactionQuerier.GetRecentUserTransactions(transactionsPageRequest, ledgerState);

        // NB - We don't return a total here as we don't have an index on user transactions
        return new RecentTransactionsResponse(
            ledgerState,
            nextCursor: results.NextPageCursor?.ToCursorString(),
            results.Transactions
        );
    }

    [HttpPost("status")]
    public async Task<TransactionStatusResponse> Status(TransactionStatusRequest request)
    {
        var transactionIdentifier = _validations.ExtractValidTransactionIdentifier(request.TransactionIdentifier);
        var ledgerState = await _ledgerStateQuerier.GetValidLedgerStateForReadRequest(request.AtStateIdentifier);

        var committedTransaction = await _transactionQuerier.LookupCommittedTransaction(transactionIdentifier, ledgerState);

        if (committedTransaction != null)
        {
            return new TransactionStatusResponse(ledgerState, committedTransaction);
        }

        var mempoolTransaction = await _transactionQuerier.LookupMempoolTransaction(transactionIdentifier);

        if (mempoolTransaction != null)
        {
            return new TransactionStatusResponse(ledgerState, mempoolTransaction);
        }

        throw new TransactionNotFoundException(request.TransactionIdentifier);
    }

    [HttpPost("build")]
    public async Task<TransactionBuildResponse> Build(TransactionBuildRequest request)
    {
        var ledgerState = await _ledgerStateQuerier.GetValidLedgerStateForConstructionRequest(request.AtStateIdentifier);
        return new TransactionBuildResponse(
            await _constructionAndSubmissionService.HandleBuildRequest(request, ledgerState)
        );
    }

    [HttpPost("finalize")]
    public async Task<TransactionFinalizeResponse> Finalize(TransactionFinalizeRequest request)
    {
        return await _constructionAndSubmissionService.HandleFinalizeRequest(request);
    }

    [HttpPost("submit")]
    public async Task<TransactionSubmitResponse> Submit(TransactionSubmitRequest request)
    {
        return await _constructionAndSubmissionService.HandleSubmitRequest(request);
    }
}
