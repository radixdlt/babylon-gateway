#nullable disable

using RadixDlt.CoreApiSdk.Api;
using RadixDlt.CoreApiSdk.Model;
using RadixDlt.NetworkGateway.Commons.Addressing;
using RadixDlt.NetworkGateway.DataAggregator.NodeServices.ApiReaders;
using RadixDlt.NetworkGateway.GatewayApi.Configuration;
using RadixDlt.NetworkGateway.GatewayApi.CoreCommunications;
using RadixDlt.NetworkGateway.GatewayApi.Services;
using RadixDlt.NetworkGateway.GatewayApiSdk.Model;
using RadixDlt.NetworkGateway.IntegrationTests.Builders;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using System.Collections.Generic;
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
    private bool _isGenesisIngested;

    public TestGlobalEntities GlobalEntities { get; set; } = new();

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
        if (!_isGenesisIngested && CoreApiStubDefaultConfiguration.CommittedGenesisTransactionsResponse != null)
        {
            _isGenesisIngested = true;
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

    public async Task<CapturedConfig> CaptureConfiguration()
    {
        var networkConfiguration = MapNetworkConfigurationResponse(CoreApiStubDefaultConfiguration.NetworkConfigurationResponse);

        return await Task.FromResult(new CapturedConfig(
            networkConfiguration.NetworkDefinition.NetworkName,
            networkConfiguration.WellKnownAddresses.XrdAddress,
            networkConfiguration.NetworkAddressHrps.ToAddressHrps(),
            new TokenIdentifier(networkConfiguration.WellKnownAddresses.XrdAddress)
        ));
    }

    private static NetworkConfiguration MapNetworkConfigurationResponse(NetworkConfigurationResponse networkConfiguration)
    {
        return new NetworkConfiguration
        {
            NetworkDefinition = new NetworkDefinition { NetworkName = networkConfiguration.Network },
            NetworkAddressHrps = new NetworkAddressHrps
            {
                AccountHrp = "account_" + networkConfiguration.NetworkHrpSuffix,
                ResourceHrpSuffix = "resource_" + networkConfiguration.NetworkHrpSuffix,
                ValidatorHrp = "validator_" + networkConfiguration.NetworkHrpSuffix,
                NodeHrp = "node_" + networkConfiguration.NetworkHrpSuffix,
            },
            WellKnownAddresses = new WellKnownAddresses
            {
                XrdAddress = RadixBech32.GenerateXrdAddress("resource_" + networkConfiguration.NetworkHrpSuffix),
            },
        };
    }
}
