#nullable disable

using RadixDlt.CoreApiSdk.Api;
using RadixDlt.CoreApiSdk.Model;
using RadixDlt.NetworkGateway.DataAggregator.NodeServices.ApiReaders;
using RadixDlt.NetworkGateway.GatewayApi.Configuration;
using RadixDlt.NetworkGateway.GatewayApi.CoreCommunications;
using RadixDlt.NetworkGateway.GatewayApi.Services;
using RadixDlt.NetworkGateway.IntegrationTests.Builders;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ICoreApiProvider = RadixDlt.NetworkGateway.DataAggregator.NodeServices.ApiReaders.ICoreApiProvider;

namespace RadixDlt.NetworkGateway.IntegrationTests.CoreApiStubs;

public class CoreApiStub :
    ICoreNodeHealthChecker,
    INetworkConfigurationReader,
    ITransactionLogReader,
    ICoreApiProvider,
    GatewayApi.CoreCommunications.ICoreApiProvider,
    ICoreApiHandler
{
    private bool _genesisWasIngested = false;

    public TestGlobalEntities GlobalEntities { get; set; } = new TestGlobalEntities();

    public CoreApiStubDefaultConfiguration CoreApiStubDefaultConfiguration { get; } = new();

    #region injected stubs

    public CoreApiNode CoreApiNode => CoreApiStubDefaultConfiguration.GatewayCoreApiNode;

    public StatusApi StatusApi => new StatusApiStub(CoreApiStubDefaultConfiguration.NetworkStatusResponse);

    public TransactionApi TransactionApi { get; }

    public TransactionApi TransactionsApi { get; }

    public Task<CoreNodeHealthResult> CheckCoreNodeHealth(CancellationToken cancellationToken)
    {
        return Task.FromResult(CoreApiStubDefaultConfiguration.CoreNodeHealthResult);
    }

    public CoreApiNode GetCoreNodeConnectedTo()
    {
        return CoreApiStubDefaultConfiguration.GatewayCoreApiNode;
    }

    public Task<NetworkConfigurationResponse> GetNetworkConfiguration(CancellationToken token)
    {
        return Task.FromResult(CoreApiStubDefaultConfiguration.NetworkConfigurationResponse);
    }

    string ICoreApiHandler.GetNetworkIdentifier()
    {
        return CoreApiStubDefaultConfiguration.NetworkName;
    }

    public Task<CommittedTransactionsResponse> GetTransactions(long stateVersion, int count, CancellationToken token)
    {
        if (!_genesisWasIngested && CoreApiStubDefaultConfiguration.CommittedGenesisTransactionsResponse != null)
        {
            _genesisWasIngested = true;
            return Task.FromResult(CoreApiStubDefaultConfiguration.CommittedGenesisTransactionsResponse);
        }

        if (CoreApiStubDefaultConfiguration.CommittedTransactionsResponse != null)
        {
            return Task.FromResult(CoreApiStubDefaultConfiguration.CommittedTransactionsResponse);
        }

        return Task.FromResult(new CommittedTransactionsResponse(transactions: new List<CommittedTransaction>()));
    }

    public Task<TransactionPreviewResponse> PreviewTransaction(
        TransactionPreviewRequest request,
        CancellationToken token = default)
    {
        return Task.FromResult(CoreApiStubDefaultConfiguration.TransactionPreviewResponse);
    }

    public Task<TransactionSubmitResponse> SubmitTransaction(
        TransactionSubmitRequest request,
        CancellationToken token = default)
    {
        return Task.FromResult(CoreApiStubDefaultConfiguration.TransactionSubmitResponse);
    }

    #endregion
}
