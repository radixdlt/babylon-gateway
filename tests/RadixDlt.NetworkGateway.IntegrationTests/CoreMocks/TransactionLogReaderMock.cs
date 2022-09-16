using Moq;
using RadixDlt.CoreApiSdk.Model;
using RadixDlt.NetworkGateway.DataAggregator.NodeServices.ApiReaders;
using RadixDlt.NetworkGateway.GatewayApi.Configuration;
using RadixDlt.NetworkGateway.GatewayApi.CoreCommunications;
using RadixDlt.NetworkGateway.IntegrationTests.Utilities;
using System.Collections.Generic;
using System.Threading;

namespace RadixDlt.NetworkGateway.IntegrationTests.CoreMocks;

public class TransactionLogReaderMock : Mock<ITransactionLogReader>
{
    public TransactionLogReaderMock()
    {
        Setup(x => x.GetTransactions(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CommittedTransactionsResponse(
                stateIdentifier: new StateIdentifier(stateVersion: 0L, transactionAccumulator: "transaction accumulator"),
                transactions: new List<CommittedTransaction>()
                {
                    new CommittedTransaction(
                        new TransactionIdentifier(hash: "hash"),
                        committedStateIdentifier: new StateIdentifier(stateVersion: 0L, transactionAccumulator: "transaction accumulator"),
                        operationGroups: new List<OperationGroup>()
                        {
                            new OperationGroup(
                                operations: new List<Operation>()
                                {
                                    new Operation(
                                        type: "operation",
                                        new EntityIdentifier(
                                            address: "address",
                                            subEntity: new SubEntity(
                                                address: "address",
                                                metadata: new SubEntityMetadata(validatorAddress: "validator address", epochUnlock: 0L)))),
                                }),
                        },
                        metadata: new CommittedTransactionMetadata(
                            size: 0,
                            hex: "0x12345",
                            fee: new ResourceAmount(
                                value: "1000",
                                resourceIdentifier: new ResourceIdentifier(type: "XRD")),
                            signedBy: new PublicKey(hex: "0x12345"),
                            message: "message"
                            )
                    ),
                }
            ));
    }
}
