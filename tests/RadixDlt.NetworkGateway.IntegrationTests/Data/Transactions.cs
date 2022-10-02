using RadixDlt.NetworkGateway.IntegrationTests.CoreApiStubs;
using RadixDlt.NetworkGateway.IntegrationTests.Utilities;
using System;
using System.Text;

namespace RadixDlt.NetworkGateway.IntegrationTests.Data;

public class Transactions
{
    private readonly CoreApiStubDefaultConfiguration _defaultConfig;

    public Transactions(CoreApiStubDefaultConfiguration defaultConfig)
    {
        _defaultConfig = defaultConfig;
    }

    public string SubmitTransactionHex
    {
        get
        {
            var transaction = $"{{\"network_identifier\": {{\"network\": \"{_defaultConfig.NetworkDefinition.LogicalName}\"}}, \"notarized_transaction\": \"10020000001002000000100200000010070000000701110f000000496e7465726e616c546573746e6574000000000a00000000000000000a64000000000000000a0600000000000000912100000002f9308a019258c31049344f85f89d5229b531c845836f99b08601f113bce036f9010010010000003011010000000d000000436c656172417574685a6f6e65000000003023020000000200000091210000000279be667ef9dcbbac55a06295ce870b07029bfcdb2dce28d959f2815b16f817989240000000d6f37bebb4c67ebb0844dd48e447c415a13b47fafdf13495f58b21826dc044a043fb00243cfe573bbb38b8ae9371801c2b91ec92ae764238e4ff40d857e58a3002000000912100000002c6047f9441ed7d6d3045406e95c07cd85c778e4b8cef3ca7abac09b95c709ee5924000000047da0da82cdceed2a227ebd305ece670cf12aaedad6863ebce173d0952eea73b3a1b136bae431d82bae822ceb11eaed406dddc1a4a94756201cb7292139584bf9240000000a767554290bd2cba8e63bc1feeefc1534ebcd33fe345f9a8d0ac76abc1d3bd5968e847ec5ca55d6e9fe18227f13c5c114463751e9bc5a38f563ba8819d7fc882\"}}";

            return Convert
                .ToHexString(Encoding.UTF8.GetBytes(transaction)).ToLowerInvariant();
        }
    }
}
