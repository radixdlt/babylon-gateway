using System;
using System.Collections;
using System.Linq;

namespace RadixDlt.NetworkGateway.IntegrationTests.CoreMocks;

public class CoreApiMocks : Hashtable
{
    public CoreApiMocks()
    {
    }

    public static CoreApiMocks CreateDefaultMocks()
    {
        var coreApiMocks = new CoreApiMocks();

        // Gateway
        AddCoreMock(coreApiMocks, new CoreApiHandlerMock().Object);
        AddCoreMock(coreApiMocks, new CoreNodeHealthCheckerMock().Object);

        // Data Aggregator
        AddCoreMock(coreApiMocks, new NetworkConfigurationReaderMock().Object);
        AddCoreMock(coreApiMocks, new TransactionLogReaderMock().Object);
        AddCoreMock(coreApiMocks, new NodeMempoolFullTransactionReaderWorkerMock().Object);
        AddCoreMock(coreApiMocks, new NodeMempoolTransactionIdsReaderWorkerMock().Object);

        return coreApiMocks;
    }

    private static void AddCoreMock<T>(CoreApiMocks coreApiMocks, T mock)
    {
        if (mock == null)
        {
            // log
            return;
        }

        // https://github.com/moq/moq4/blob/main/src/Moq/Mock.cs
        var imockedType = mock.GetType().GetInterfaces()
            .First(i => i.Name.Equals("IMocked`1", StringComparison.Ordinal));

        var mockedType = imockedType.GetGenericArguments()[0];

        coreApiMocks.Add(mockedType, mock);
    }
}
