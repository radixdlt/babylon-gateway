#nullable disable

using Microsoft.VisualStudio.TestPlatform.Utilities;
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
using Xunit.Sdk;
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
        var stateUpdatesList = new List<StateUpdates>();

        var sysFaucetBlueprint = new BlueprintBuilder().Of(typeof(SysFaucetBlueprint)).Build();

        var package = new PackageBuilder()
            .WithBlueprints(new List<StateUpdates> { sysFaucetBlueprint })
            .Build();

        stateUpdatesList.Add(package);

        var component = new ComponentBuilder()
            .WithComponent(new SysFaucetComponent())
            .Build();

        stateUpdatesList.Add(component);

        var vault = new VaultBuilder().Build();

        stateUpdatesList.Add(vault);

        var payload = stateUpdatesList.ToJson();

        CommittedTransactionsResponse response = new(
            fromStateVersion: 1L,
            toStateVersion: 1L,
            maxStateVersion: 1L,
            transactions: new List<CommittedTransaction>()
            {
                new CommittedTransaction(
                    stateVersion: 1L, notarizedTransaction: null,
                    receipt: new TransactionReceipt(
                        status: TransactionStatus.Succeeded,
                        feeSummary: new FeeSummary(
                            loanFullyRepaid: true,
                            costUnitLimit: 10000000,
                            costUnitConsumed: 0,
                            costUnitPriceAttos: "1000000000000",
                            tipPercentage: 0,
                            xrdBurnedAttos: "0",
                            xrdTippedAttos: "0"
                        ),
                        stateUpdates: stateUpdatesList.Combine(),
                        output: new List<SborData>(),
                        errorMessage: "error"
                    )
                ),
            }
        );

        // return Task.FromResult(CoreApiStubDefaultConfiguration.CommittedTransactionsResponse);

        return Task.FromResult(response);
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
