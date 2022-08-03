using RadixDlt.NetworkGateway.Contracts.Api.Model;
using RadixDlt.NetworkGateway.Services;

namespace RadixDlt.NetworkGateway.Endpoints;

public class TransactionEndpoint
{
    private readonly ILedgerStateQuerier _ledgerStateQuerier;
    private readonly ITransactionQuerier _transactionQuerier;

    public TransactionEndpoint(ILedgerStateQuerier ledgerStateQuerier, ITransactionQuerier transactionQuerier)
    {
        _ledgerStateQuerier = ledgerStateQuerier;
        _transactionQuerier = transactionQuerier;
    }

    public async Task<RecentTransactionsResponse> Recent(RecentTransactionsRequest request, CancellationToken token = default)
    {
        request.AtStateIdentifier = new PartialLedgerStateIdentifier(100);

        var ledgerState = await _ledgerStateQuerier.GetValidLedgerStateForReadRequest(request.AtStateIdentifier);

        // var unvalidatedLimit = request.Limit is default(int) ? 10 : request.Limit;

        var transactionsPageRequest = new RecentTransactionPageRequest(
            Cursor: CommittedTransactionPaginationCursor.FromCursorString(request.Cursor),
            PageSize: 10 // _validations.ExtractValidIntInBoundInclusive("Page size", unvalidatedLimit, 1, _gatewayApiConfiguration.GetMaxPageSize())
        );

        var results = await _transactionQuerier.GetRecentUserTransactions(transactionsPageRequest, ledgerState);

        // NB - We don't return a total here as we don't have an index on user transactions
        return new RecentTransactionsResponse(
            ledgerState,
            nextCursor: results.NextPageCursor?.ToCursorString(),
            results.Transactions
        );
    }

    public async Task<TransactionBuildResponse> Build(TransactionBuildRequest request, CancellationToken token = default)
    {
        request.DoSth();

        await Task.Delay(1, token);

        var fee = new TokenAmount("123", new TokenIdentifier("some rri"));

        return new TransactionBuildResponse(new TransactionBuild(fee, "unsigned trans", "payload to sign"));
    }
}
