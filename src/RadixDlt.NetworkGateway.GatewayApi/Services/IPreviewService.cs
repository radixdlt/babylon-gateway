using Microsoft.Extensions.Logging;
using RadixDlt.NetworkGateway.Commons.Extensions;
using RadixDlt.NetworkGateway.GatewayApi.CoreCommunications;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.GatewayApi.Services;

using CoreModel = RadixDlt.CoreApiSdk.Model;
using GatewayModel = RadixDlt.NetworkGateway.GatewayApiSdk.Model;

public interface IPreviewService
{
    Task<GatewayModel.TransactionPreviewResponse> HandlePreviewRequest(GatewayModel.TransactionPreviewRequest request, CancellationToken token = default);
}

internal class PreviewService : IPreviewService
{
    private readonly ICoreApiHandler _coreApiHandler;
    private readonly IEnumerable<IPreviewServiceObserver> _observers;
    private readonly ILogger _logger;

    public PreviewService(ICoreApiHandler coreApiHandler, IEnumerable<IPreviewServiceObserver> observers, ILogger<PreviewService> logger)
    {
        _coreApiHandler = coreApiHandler;
        _observers = observers;
        _logger = logger;
    }

    public async Task<GatewayModel.TransactionPreviewResponse> HandlePreviewRequest(GatewayModel.TransactionPreviewRequest request, CancellationToken token = default)
    {
        try
        {
            await _observers.ForEachAsync(x => x.PreHandlePreviewRequest(request));

            var response = await HandlePreviewAndCreateResponse(request, token);

            await _observers.ForEachAsync(x => x.PostHandlePreviewRequest(request, response));

            return response;
        }
        catch (Exception ex)
        {
            await _observers.ForEachAsync(x => x.HandlePreviewRequestFailed(request, ex));

            throw;
        }
    }

    private async Task<GatewayModel.TransactionPreviewResponse> HandlePreviewAndCreateResponse(GatewayModel.TransactionPreviewRequest request, CancellationToken token)
    {
        // consider this a mock/dumb implementation for testing purposes only

        using var timeoutTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(3)); // TODO configurable
        using var finalTokenSource = CancellationTokenSource.CreateLinkedTokenSource(timeoutTokenSource.Token, token);

        var result = await _coreApiHandler.PreviewTransaction(
            new CoreModel.TransactionPreviewRequest(
                _coreApiHandler.GetNetworkIdentifier(),
                request.Manifest,
                request.CostUnitLimit,
                request.TipPercentage,
                request.Nonce,
                request.SignerPublicKeys,
                new CoreModel.TransactionPreviewRequestFlags(request.Flags.UnlimitedLoan)
            ),
            finalTokenSource.Token
        );

        // TODO implement
        _logger.LogInformation("Temporary logging entire payload: {Result}", result);

        var receipt = new GatewayModel.TransactionReceipt("TBD");
        var changes = new List<GatewayModel.ResourceChange>
        {
            new("TBD"),
        };

        return new GatewayModel.TransactionPreviewResponse(receipt, changes);
    }
}
