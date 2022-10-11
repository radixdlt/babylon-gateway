#nullable disable

using RadixDlt.CoreApiSdk.Api;
using RadixDlt.CoreApiSdk.Model;
using RadixDlt.NetworkGateway.Commons.Addressing;
using RadixDlt.NetworkGateway.DataAggregator.NodeServices.ApiReaders;
using RadixDlt.NetworkGateway.GatewayApi.Configuration;
using RadixDlt.NetworkGateway.GatewayApi.CoreCommunications;
using RadixDlt.NetworkGateway.GatewayApi.Services;
using RadixDlt.NetworkGateway.GatewayApiSdk.Model;
using RadixDlt.NetworkGateway.IntegrationTests.Data;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using System.Threading;
using System.Threading.Tasks;
using ICoreApiProvider = RadixDlt.NetworkGateway.DataAggregator.NodeServices.ApiReaders.ICoreApiProvider;
using TransactionPreviewRequest = RadixDlt.CoreApiSdk.Model.TransactionPreviewRequest;
using TransactionPreviewResponse = RadixDlt.CoreApiSdk.Model.TransactionPreviewResponse;
using TransactionSubmitRequest = RadixDlt.CoreApiSdk.Model.TransactionSubmitRequest;
using TransactionSubmitResponse = RadixDlt.CoreApiSdk.Model.TransactionSubmitResponse;

namespace RadixDlt.NetworkGateway.IntegrationTests.CoreApiStubs;

public class CoreApiStub :
    ICoreNodeHealthChecker,
    INetworkConfigurationReader,
    ITransactionLogReader,
    ICoreApiProvider,
    GatewayApi.CoreCommunications.ICoreApiProvider,
    ICoreApiHandler,
    ICapturedConfigProvider
{
    private readonly TestTransactionStreamStore _transactionStreamStore;

    public CoreApiStubRequestsAndResponses RequestsAndResponses { get; }

    public CoreApiStub(CoreApiStubRequestsAndResponses coreApiStubRequestsAndResponses, TestTransactionStreamStore transactionStreamStore)
    {
        _transactionStreamStore = transactionStreamStore;
        RequestsAndResponses = coreApiStubRequestsAndResponses;
    }

    #region injected stubs

    public CoreApiNode CoreApiNode => RequestsAndResponses.GatewayCoreApiNode;

    public StatusApi StatusApi => new StatusApiStub(RequestsAndResponses.NetworkStatusResponse);

    public TransactionApi TransactionApi { get; }

    public TransactionApi TransactionsApi { get; }

    public Task<CoreNodeHealthResult> CheckCoreNodeHealth(CancellationToken cancellationToken)
    {
        return Task.FromResult(RequestsAndResponses.CoreNodeHealthResult);
    }

    public CoreApiNode GetCoreNodeConnectedTo()
    {
        return RequestsAndResponses.GatewayCoreApiNode;
    }

    public Task<NetworkConfigurationResponse> GetNetworkConfiguration(CancellationToken token)
    {
        return Task.FromResult(RequestsAndResponses.NetworkConfigurationResponse);
    }

    string ICoreApiHandler.GetNetworkIdentifier()
    {
        return GenesisData.NetworkDefinition.LogicalName;
    }

    public Task<CommittedTransactionsResponse> GetTransactions(long stateVersion, int count, CancellationToken token)
    {
        return _transactionStreamStore.GetTransactions(stateVersion, count);
    }

    public Task<TransactionPreviewResponse> PreviewTransaction(
        TransactionPreviewRequest request,
        CancellationToken token = default)
    {
        return Task.FromResult(RequestsAndResponses.TransactionPreviewResponse);
    }

    public Task<TransactionSubmitResponse> SubmitTransaction(
        TransactionSubmitRequest request,
        CancellationToken token = default)
    {
        return Task.FromResult(RequestsAndResponses.TransactionSubmitResponse);
    }

    #endregion

    public async Task<CapturedConfig> CaptureConfiguration()
    {
        var networkConfiguration = MapNetworkConfigurationResponse(RequestsAndResponses.NetworkConfigurationResponse);

        return await Task.FromResult(new CapturedConfig(
            networkConfiguration.NetworkName,
            networkConfiguration.NetworkConfigurationWellKnownAddresses.XrdAddress,
            networkConfiguration.NetworkConfigurationHrpDefinition.CreateDefinition(),
            new TokenIdentifier(networkConfiguration.NetworkConfigurationWellKnownAddresses.XrdAddress)
        ));
    }

    private static NetworkConfiguration MapNetworkConfigurationResponse(NetworkConfigurationResponse networkConfiguration)
    {
        var hrpSuffix = networkConfiguration.NetworkHrpSuffix;

        return new NetworkConfiguration
        {
            NetworkName = networkConfiguration.Network,
            NetworkConfigurationHrpDefinition = new NetworkConfigurationHrpDefinition
            {
                PackageHrp = $"package_{hrpSuffix}",
                NormalComponentHrp = $"component_{hrpSuffix}",
                AccountComponentHrp = $"account_{hrpSuffix}",
                SystemComponentHrp = $"system_{hrpSuffix}",
                ResourceHrp = $"resource_{hrpSuffix}",
                ValidatorHrp = $"validator_{hrpSuffix}",
                NodeHrp = $"node_{hrpSuffix}",
            },
            NetworkConfigurationWellKnownAddresses = new NetworkConfigurationWellKnownAddresses
            {
                XrdAddress = RadixBech32.GenerateXrdAddress("resource_" + networkConfiguration.NetworkHrpSuffix),
            },
        };
    }
}
