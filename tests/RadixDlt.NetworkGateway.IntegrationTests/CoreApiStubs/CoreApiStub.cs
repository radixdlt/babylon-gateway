using RadixDlt.CoreApiSdk.Api;
using RadixDlt.CoreApiSdk.Model;
using RadixDlt.NetworkGateway.DataAggregator.NodeServices.ApiReaders;
using RadixDlt.NetworkGateway.GatewayApi.Configuration;
using RadixDlt.NetworkGateway.GatewayApi.CoreCommunications;
using RadixDlt.NetworkGateway.GatewayApi.Services;
using System.Threading;
using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.IntegrationTests.CoreApiStubs;

public class CoreApiStub :
    ICoreNodeHealthChecker,
    INetworkConfigurationReader,
    ITransactionLogReader,
    DataAggregator.NodeServices.ApiReaders.ICoreApiProvider,
    GatewayApi.CoreCommunications.ICoreApiProvider,
    ICoreApiHandler
{
    public CoreApiStubDefaultConfiguration CoreApiStubDefaultConfiguration { get; } = new();

    #region injected stubs
    public Task<CoreNodeHealthResult> CheckCoreNodeHealth(CancellationToken cancellationToken)
    {
        return Task.FromResult(CoreApiStubDefaultConfiguration.CoreNodeHealthResult);
    }

    public Task<NetworkConfigurationResponse> GetNetworkConfiguration(CancellationToken token)
    {
        return Task.FromResult(CoreApiStubDefaultConfiguration.NetworkConfigurationResponse);
    }

    public Task<CommittedTransactionsResponse> GetTransactions(long stateVersion, int count, CancellationToken token)
    {
        return Task.FromResult(CoreApiStubDefaultConfiguration.CommittedTransactionsResponse);
    }

    public ConstructionApi ConstructionApi
    {
        get
        {
            return new ConstructionApi();
        }
    }

    public TransactionsApi TransactionsApi
    {
        get
        {
            return new TransactionsApi();
        }
    }

    public MempoolApi MempoolApi
    {
        get
        {
            return new MempoolApiStub(CoreApiStubDefaultConfiguration.MempoolResponse);
        }
    }

    public NetworkApi NetworkApi
    {
        get
        {
            return new NetworkApi();
        }
    }

    public CoreApiNode CoreApiNode
    {
        get
        {
            return CoreApiStubDefaultConfiguration.GatewayCoreApiNode;
        }
    }

    public NetworkIdentifier GetNetworkIdentifier()
    {
        return CoreApiStubDefaultConfiguration.NetworkIdentifier;
    }

    public CoreApiNode GetCoreNodeConnectedTo()
    {
        return CoreApiStubDefaultConfiguration.GatewayCoreApiNode;
    }

    public Task<ConstructionBuildResponse> BuildTransaction(ConstructionBuildRequest request, CancellationToken token = default)
    {
        return Task.FromResult(CoreApiStubDefaultConfiguration.ConstructionBuildResponse);
    }

    public Task<ConstructionParseResponse> ParseTransaction(ConstructionParseRequest request, CancellationToken token = default)
    {
        return Task.FromResult(CoreApiStubDefaultConfiguration.ConstructionParseResponse);
    }

    public Task<ConstructionFinalizeResponse> FinalizeTransaction(ConstructionFinalizeRequest request, CancellationToken token = default)
    {
        return Task.FromResult(CoreApiStubDefaultConfiguration.ConstructionFinalizeResponse);
    }

    public Task<ConstructionHashResponse> GetTransactionHash(ConstructionHashRequest request, CancellationToken token = default)
    {
        return Task.FromResult(CoreApiStubDefaultConfiguration.ConstructionHashResponse);
    }

    public Task<ConstructionSubmitResponse> SubmitTransaction(ConstructionSubmitRequest request, CancellationToken token = default)
    {
        return Task.FromResult(CoreApiStubDefaultConfiguration.ConstructionSubmitResponse);
    }
    #endregion
}
