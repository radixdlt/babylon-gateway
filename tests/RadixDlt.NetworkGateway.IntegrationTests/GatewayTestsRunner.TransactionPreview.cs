using RadixDlt.CoreApiSdk.Model;
using RadixDlt.NetworkGateway.IntegrationTests.Builders;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Text;
using PublicKey = RadixDlt.NetworkGateway.GatewayApiSdk.Model.PublicKey;
using TransactionPreviewRequest = RadixDlt.NetworkGateway.GatewayApiSdk.Model.TransactionPreviewRequest;
using TransactionPreviewRequestFlags = RadixDlt.NetworkGateway.GatewayApiSdk.Model.TransactionPreviewRequestFlags;

namespace RadixDlt.NetworkGateway.IntegrationTests;

public partial class GatewayTestsRunner
{
    public GatewayTestsRunner TransactionPreviewBuild(
        string manifest,
        long costUnitLimit,
        long tipPercentage,
        string nonce,
        List<PublicKey> signerPublicKeys,
        TransactionPreviewRequestFlags flags)
    {
        _testConsole.WriteLine(MethodBase.GetCurrentMethod()!.Name);

        // build TransactionPreviewRequest
        CoreApiStub.CoreApiStubDefaultConfiguration.TransactionPreviewRequest = new TransactionPreviewRequest(
            manifest: manifest,
            blobsHex: new List<string> { "blob hex" },
            costUnitLimit: costUnitLimit,
            tipPercentage: tipPercentage,
            nonce: nonce,
            signerPublicKeys: signerPublicKeys,
            flags: flags
        );

        var json = CoreApiStub.CoreApiStubDefaultConfiguration.TransactionPreviewRequest.ToJson();
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _request = ("/transaction/preview", content);

        // build TransactionPreviewResponse
        var stateUpdatesList = new List<StateUpdates>();

        var (_, tokenStates) = new FungibleResourceBuilder(CoreApiStub.CoreApiStubDefaultConfiguration)
            .WithResourceName("PreviewToken")
            .Build();

        stateUpdatesList.Add(tokenStates);

        TransactionReceipt transactionReceipt = new TransactionReceiptBuilder()
            .WithStateUpdates(stateUpdatesList.Combine())
            .WithTransactionStatus(TransactionStatus.Succeeded)
            .Build();

        CoreApiStub.CoreApiStubDefaultConfiguration.TransactionPreviewResponse = new TransactionPreviewResponse(
            transactionReceipt,
            new List<ResourceChange>()
            {
                new(
                    "resource address",
                    "component address",
                    new EntityId(EntityType.Component, "entity address"),
                    amountAttos: "0"),
            },
            logs: new List<TransactionPreviewResponseLogsInner>()
            {
                new("level: debug", "message"),
            });

        return this;
    }
}
