using RadixDlt.CoreApiSdk.Model;
using RadixDlt.NetworkGateway.Commons.Model;
using RadixDlt.NetworkGateway.IntegrationTests.Builders;
using RadixDlt.NetworkGateway.IntegrationTests.CoreApiStubs;
using RadixDlt.NetworkGateway.IntegrationTests.Utilities;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using EcdsaSecp256k1PublicKey = RadixDlt.NetworkGateway.GatewayApiSdk.Model.EcdsaSecp256k1PublicKey;
using PublicKeyType = RadixDlt.NetworkGateway.GatewayApiSdk.Model.PublicKeyType;
using TransactionStatus = RadixDlt.NetworkGateway.GatewayApiSdk.Model.TransactionStatus;

namespace RadixDlt.NetworkGateway.IntegrationTests;

public class GatewayTestsRunner : IDisposable
{
    private TestGatewayApiFactory? _gatewayApiFactory;
    private TestDataAggregatorFactory? _dataAggregatorFactory;

    private CoreApiStub _coreApiStub;

    public GatewayTestsRunner()
    {
        _coreApiStub = new CoreApiStub();
    }

    public async Task<GatewayTestsRunner> WaitUntilAllTransactionsAreIngested(TimeSpan? timeout = null)
    {
        await WaitAsync(timeout);

        return this;
    }

    public async Task<T> ActAsync<T>(string? requestUri, HttpContent? content)
    {
        if (requestUri == null)
        {
            throw new Exception("Gateway api uri is missing.");
        }

        if (_gatewayApiFactory == null)
        {
            throw new Exception("Gateway http client is not initialized.");
        }

        using var response = await _gatewayApiFactory.Client.PostAsync(requestUri, content);

        var payload = await response.ParseToObjectAndAssert<T>();

        return payload;
    }

    public GatewayTestsRunner MockGenesis()
    {
        var networkConfiguration = _coreApiStub.CoreApiStubDefaultConfiguration.NetworkConfigurationResponse;

        var (tokenEntity, tokens) = new FungibleResourceBuilder(networkConfiguration)
            .WithResourceName("XRD Tokens")
            .Build();

        _coreApiStub.GlobalEntities.Add(tokenEntity);
        _coreApiStub.GlobalEntities.AddStateUpdates(tokens);

        var (vaultEntity, vault) = new VaultBuilder(_coreApiStub.CoreApiStubDefaultConfiguration.NetworkConfigurationResponse)
            .WithVaultName("SysFaucet Vault")
            .WithFungibleTokens(tokenEntity.GlobalAddress)
            .Build();

        _coreApiStub.GlobalEntities.Add(vaultEntity); // only for testing purposes, vault is not a global entity!

        _coreApiStub.GlobalEntities.AddStateUpdates(vault);
        var (packageEntity, package) = new PackageBuilder()
            .WithBlueprints(new List<IBlueprint> { new SysFaucetBlueprint() })
            .Build();

        _coreApiStub.GlobalEntities.Add(packageEntity);
        _coreApiStub.GlobalEntities.AddStateUpdates(package);

        var (componentEntity, component) = new ComponentBuilder(_coreApiStub.CoreApiStubDefaultConfiguration.NetworkConfigurationResponse)
            .WithComponentName("SysFaucet component")
            .WithComponentInfoSubstate(packageEntity.GlobalAddress, packageEntity.Name)
            .WithVault(vaultEntity.EntityAddressHex)
            .Build();

        _coreApiStub.GlobalEntities.Add(componentEntity);
        _coreApiStub.GlobalEntities.AddStateUpdates(component);

        var transactionReceipt = new TransactionReceiptBuilder().WithStateUpdates(_coreApiStub.GlobalEntities.StateUpdates).Build();

        _coreApiStub.CoreApiStubDefaultConfiguration.CommittedGenesisTransactionsResponse = new(
            fromStateVersion: 1L,
            toStateVersion: 1L,
            maxStateVersion: 363L,
            transactions: new List<CommittedTransaction>()
            {
                new(
                    stateVersion: 1L, notarizedTransaction: null,
                    receipt: transactionReceipt
                ),
            }
        );

        return this;
    }

    public CoreApiStub ArrangeMempoolTransactionStatusTest(string databaseName, TransactionStatus.StatusEnum expectedStatus)
    {
        switch (expectedStatus)
        {
            case TransactionStatus.StatusEnum.FAILED:
                _coreApiStub.CoreApiStubDefaultConfiguration.MempoolTransactionStatus =
                    MempoolTransactionStatus.Failed;
                break;
            case TransactionStatus.StatusEnum.PENDING:
                _coreApiStub.CoreApiStubDefaultConfiguration.MempoolTransactionStatus =
                    MempoolTransactionStatus.SubmittedOrKnownInNodeMempool;
                break;
            case TransactionStatus.StatusEnum.CONFIRMED:
                _coreApiStub.CoreApiStubDefaultConfiguration.MempoolTransactionStatus =
                    MempoolTransactionStatus.Committed;
                break;
        }

        Initialize(databaseName);

        return _coreApiStub;
    }

    public CoreApiStub ArrangeSubmittedTransactionStatusTest(string databaseName)
    {
        Initialize(databaseName);

        return _coreApiStub;
    }

    public CoreApiStub ArrangeSubmitTransactionTest(string databaseName)
    {
        Initialize(databaseName);

        return _coreApiStub;
    }

    public CoreApiStub ArrangeGatewayVersionsTest(string databaseName)
    {
        Initialize(databaseName);

        // set custom gatewayApi and openSchemaApi versions

        return _coreApiStub;
    }

    public CoreApiStub ArrangeTransactionRecentTest(string databaseName)
    {
        Initialize(databaseName);

        // set custom transaction data

        return _coreApiStub;
    }

    public CoreApiStub ArrangeTransactionPreviewTest(string databaseName)
    {
        Initialize(databaseName);

        // set preview request
        _coreApiStub.CoreApiStubDefaultConfiguration.TransactionPreviewRequest = new GatewayApiSdk.Model.TransactionPreviewRequest(
            manifest: new ManifestBuilder().CallMethod("021c77780d10210ec9f0ea4a372ab39e09f2222c07c9fb6e5cfc81", "CALL_FUNCTION").Build(),
            blobsHex: new List<string>() { "blob hex" },
            costUnitLimit: 1L,
            tipPercentage: 5L,
            nonce: "nonce",
            signerPublicKeys: new List<GatewayApiSdk.Model.PublicKey>()
            {
                new(new EcdsaSecp256k1PublicKey(
                    keyType: PublicKeyType.EcdsaSecp256k1,
                    keyHex: "010000000000000000000000000000000000000000000000000001")),
            },
            flags: new GatewayApiSdk.Model.TransactionPreviewRequestFlags(unlimitedLoan: false));

        return _coreApiStub;
    }

    public void Initialize(string databaseName)
    {
        _gatewayApiFactory = TestGatewayApiFactory.Create(_coreApiStub, databaseName);

        // allow db creation
        Task t = WaitAsync(TimeSpan.FromSeconds(5));

        _dataAggregatorFactory = TestDataAggregatorFactory.Create(_coreApiStub, databaseName);
    }

    // Tear down
    public void TearDown()
    {
        if (_dataAggregatorFactory != null)
        {
            _dataAggregatorFactory.Server.Dispose();
            _dataAggregatorFactory.Dispose();
            _dataAggregatorFactory = null;
        }

        if (_gatewayApiFactory != null)
        {
            _gatewayApiFactory.Server.Dispose();
            _gatewayApiFactory.Dispose();
            _gatewayApiFactory = null;
        }
    }

    public void Dispose()
    {
        TearDown();
    }

    private async Task WaitAsync(TimeSpan? timeout = null)
    {
        timeout ??= TimeSpan.FromSeconds(10);

        await Task.Delay(timeout.Value);
    }
}
