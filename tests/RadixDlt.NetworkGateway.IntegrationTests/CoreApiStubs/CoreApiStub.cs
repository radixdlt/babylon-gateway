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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ICoreApiProvider = RadixDlt.NetworkGateway.DataAggregator.NodeServices.ApiReaders.ICoreApiProvider;
using NetworkDefinition = RadixDlt.NetworkGateway.PostgresIntegration.Models.NetworkDefinition;
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
