using Moq;
using RadixDlt.CoreApiSdk.Model;
using RadixDlt.NetworkGateway.GatewayApi.Configuration;
using RadixDlt.NetworkGateway.GatewayApi.CoreCommunications;
using RadixDlt.NetworkGateway.IntegrationTests.Utilities;
using System.Collections.Generic;
using System.Threading;

namespace RadixDlt.NetworkGateway.IntegrationTests.CoreMocks;

public class CoreApiHandlerMock : Mock<ICoreApiHandler>
{
    public CoreApiHandlerMock()
    {
        Setup(x => x.BuildTransaction(It.IsAny<ConstructionBuildRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ConstructionBuildResponse(
                unsignedTransaction: "unsigned transaction",
                payloadToSign: "payload to sign"));

        Setup(x => x.FinalizeTransaction(It.IsAny<ConstructionFinalizeRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ConstructionFinalizeResponse(
                signedTransaction: "signed transaction"));

        Setup(x => x.ParseTransaction(It.IsAny<ConstructionParseRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ConstructionParseResponse(
                operationGroups: new List<OperationGroup>(),
                metadata: new ParsedTransactionMetadata(
                    fee: new ResourceAmount(
                        value: "1000",
                        resourceIdentifier: new ResourceIdentifier(type: "token")),
                    message: "default message")));

        Setup(x => x.SubmitTransaction(It.IsAny<ConstructionSubmitRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ConstructionSubmitResponse(
                transactionIdentifier: new TransactionIdentifier(hash: "hash")));

        Setup(x => x.GetNetworkIdentifier())
            .Returns(new NetworkIdentifier(network: DbSeedHelper.NetworkName));

        Setup(x => x.GetTransactionHash(It.IsAny<ConstructionHashRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ConstructionHashResponse(
                transactionIdentifier: new TransactionIdentifier(hash: "hash")));

        Setup(x => x.GetCoreNodeConnectedTo())
            .Returns(new CoreApiNode());
    }
}
