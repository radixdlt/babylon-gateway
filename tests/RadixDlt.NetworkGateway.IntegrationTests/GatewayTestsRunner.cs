using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RadixDlt.CoreApiSdk.Model;
using RadixDlt.NetworkGateway.Commons.Model;
using RadixDlt.NetworkGateway.IntegrationTests.Builders;
using RadixDlt.NetworkGateway.IntegrationTests.CoreApiStubs;
using RadixDlt.NetworkGateway.IntegrationTests.Data;
using RadixDlt.NetworkGateway.IntegrationTests.Utilities;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Xunit.Abstractions;
using EcdsaSecp256k1PublicKey = RadixDlt.NetworkGateway.GatewayApiSdk.Model.EcdsaSecp256k1PublicKey;
using PublicKeyType = RadixDlt.NetworkGateway.GatewayApiSdk.Model.PublicKeyType;
using TransactionStatus = RadixDlt.NetworkGateway.GatewayApiSdk.Model.TransactionStatus;

namespace RadixDlt.NetworkGateway.IntegrationTests;

public class GatewayTestsRunner : IDisposable
{
    private readonly ITestOutputHelper _testConsole;
    private TestGatewayApiFactory? _gatewayApiFactory;
    private TestDataAggregatorFactory? _dataAggregatorFactory;

    private CoreApiStub _coreApiStub;

    public GatewayTestsRunner(ITestOutputHelper testConsole)
    {
        _testConsole = testConsole;
        _coreApiStub = new CoreApiStub();
    }

    public async Task<GatewayTestsRunner> WaitUntilAllTransactionsAreIngested(TimeSpan? timeout = null)
    {
        _testConsole.WriteLine(MethodBase.GetCurrentMethod()!.NameFromAsync());

        timeout ??= TimeSpan.FromSeconds(10);

        await WaitAsync(timeout.Value);

        return this;
    }

    public async Task<T> ActAsync<T>(string? requestUri, HttpContent? content)
    {
        _testConsole.WriteLine(MethodBase.GetCurrentMethod()!.NameFromAsync());

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
        _testConsole.WriteLine(MethodBase.GetCurrentMethod()!.Name);

        var networkConfiguration = _coreApiStub.CoreApiStubDefaultConfiguration.NetworkConfigurationResponse;

        _testConsole.WriteLine("XRD resource");
        var (tokenEntity, tokens) = new FungibleResourceBuilder(networkConfiguration)
            .WithResourceName("XRD")
            .Build();

        _coreApiStub.GlobalEntities.Add(tokenEntity);
        _coreApiStub.GlobalEntities.AddStateUpdates(tokens);

        _testConsole.WriteLine("SysFaucet vault");
        var (vaultEntity, vault) = new VaultBuilder(_coreApiStub.CoreApiStubDefaultConfiguration.NetworkConfigurationResponse)
            .WithVaultName("SysFaucet vault")
            .WithFungibleTokens(tokenEntity.GlobalAddress)
            .Build();

        _coreApiStub.GlobalEntities.Add(vaultEntity); // only for testing purposes, vault is not a global entity!

        _coreApiStub.GlobalEntities.AddStateUpdates(vault);

        _testConsole.WriteLine("SysFaucet package");
        var (packageEntity, package) = new PackageBuilder()
            .WithBlueprints(new List<IBlueprint> { new SysFaucetBlueprint() })
            .WithFixedAddress(GenesisData.SysFaucetPackageAddress)
            .Build();

        _coreApiStub.GlobalEntities.Add(packageEntity);
        _coreApiStub.GlobalEntities.AddStateUpdates(package);

        // TODO: KeyValueStore builder !!!
        _testConsole.WriteLine("SysFaucet component");
        var (componentEntity, component) = new ComponentBuilder(_coreApiStub.CoreApiStubDefaultConfiguration.NetworkConfigurationResponse)
            .WithComponentName(GenesisData.SysFaucetBlueprintName)
            .WithComponentInfoSubstate(GenesisData.SysFaucetInfoSubstate)
            .WithComponentStateSubstate(GenesisData.SysFaucetStateSubstate(vaultEntity.EntityAddressHex, "000000000000000000000000000000000000000000000000000000000000000001000000"))
            .Build();

        _coreApiStub.GlobalEntities.Add(componentEntity);
        _coreApiStub.GlobalEntities.AddStateUpdates(component);

        _testConsole.WriteLine("Transaction receipt");
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

    public GatewayTestsRunner WithAccount(string accountName, string token, long balance)
    {
        _testConsole.WriteLine(MethodBase.GetCurrentMethod()!.Name);

        _testConsole.WriteLine($"Account: {accountName}, {token} {balance}");

        var (accountEntity, account) = new AccountBuilder(_coreApiStub.CoreApiStubDefaultConfiguration, _coreApiStub.GlobalEntities)
            .WithAccountName(accountName)
            .WithPublicKey(AddressHelper.GenerateRandomPublicKey())
            .WithTokenName(token)
            .WithBalance(balance)
            .Build();

        _coreApiStub.GlobalEntities.Add(accountEntity);
        _coreApiStub.GlobalEntities.AddStateUpdates(account);

        return this;
    }

    public CoreApiStub ArrangeMempoolTransactionStatusTest(string databaseName, TransactionStatus.StatusEnum expectedStatus)
    {
        _testConsole.WriteLine(MethodBase.GetCurrentMethod()!.Name);

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
        _testConsole.WriteLine(MethodBase.GetCurrentMethod()!.Name);

        Initialize(databaseName);

        return _coreApiStub;
    }

    public CoreApiStub ArrangeSubmitTransactionTest(string databaseName)
    {
        _testConsole.WriteLine(MethodBase.GetCurrentMethod()!.Name);

        Initialize(databaseName);

        return _coreApiStub;
    }

    public CoreApiStub ArrangeGatewayVersionsTest(string databaseName)
    {
        _testConsole.WriteLine(MethodBase.GetCurrentMethod()!.Name);

        Initialize(databaseName);

        // set custom gatewayApi and openSchemaApi versions

        return _coreApiStub;
    }

    public CoreApiStub ArrangeTransactionRecentTest(string databaseName)
    {
        _testConsole.WriteLine(MethodBase.GetCurrentMethod()!.Name);

        Initialize(databaseName);

        // set custom transaction data

        return _coreApiStub;
    }

    public CoreApiStub ArrangeTransactionPreviewTest(string databaseName)
    {
        _testConsole.WriteLine(MethodBase.GetCurrentMethod()!.Name);

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
                    keyHex: "010000000000000000000000000000001")),
            },
            flags: new GatewayApiSdk.Model.TransactionPreviewRequestFlags(unlimitedLoan: false));

        return _coreApiStub;
    }

    public void Initialize(string databaseName)
    {
        _testConsole.WriteLine(MethodBase.GetCurrentMethod()!.Name);

        _testConsole.WriteLine("Initializing TestGatewayApiFactory");
        _gatewayApiFactory = TestGatewayApiFactory.Create(_coreApiStub, databaseName);

        // allow db creation
        Task t = WaitAsync(TimeSpan.FromSeconds(10));
        t.Wait();

        _testConsole.WriteLine("Initializing TestDataAggregatorFactory");
        _dataAggregatorFactory = TestDataAggregatorFactory.Create(_coreApiStub, databaseName);
    }

    // Tear down
    public void TearDown()
    {
        if (_dataAggregatorFactory != null)
        {
            _testConsole.WriteLine("Tearing down TestDataAggregatorFactory");
            _dataAggregatorFactory.Server.Dispose();
            _dataAggregatorFactory.Dispose();
            _dataAggregatorFactory = null;
        }

        if (_gatewayApiFactory != null)
        {
            _testConsole.WriteLine("Tearing down TestGatewayApiFactory");
            _gatewayApiFactory.Server.Dispose();
            _gatewayApiFactory.Dispose();
            _gatewayApiFactory = null;
        }
    }

    public void Dispose()
    {
        TearDown();
    }

    public GatewayTestsRunner WithTestHeader(string testName)
    {
        _testConsole.WriteLine($"\n{new string('-', 50)}");
        _testConsole.WriteLine($"{testName} test");
        _testConsole.WriteLine($"{new string('-', 40)}");

        return this;
    }

    private async Task WaitAsync(TimeSpan timeout)
    {
        await Task.Delay(timeout);
    }
}
