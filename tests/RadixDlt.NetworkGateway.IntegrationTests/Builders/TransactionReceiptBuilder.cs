using RadixDlt.CoreApiSdk.Model;

namespace RadixDlt.NetworkGateway.IntegrationTests.Builders;

public class TransactionReceiptBuilder : IBuilder<TransactionReceipt>
{
    public TransactionReceipt Build()
    {
        return new TransactionReceipt();
    }
}
