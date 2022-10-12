/* Copyright 2021 Radix Publishing Ltd incorporated in Jersey (Channel Islands).
 *
 * Licensed under the Radix License, Version 1.0 (the "License"); you may not use this
 * file except in compliance with the License. You may obtain a copy of the License at:
 *
 * radixfoundation.org/licenses/LICENSE-v1
 *
 * The Licensor hereby grants permission for the Canonical version of the Work to be
 * published, distributed and used under or by reference to the Licensor’s trademark
 * Radix ® and use of any unregistered trade names, logos or get-up.
 *
 * The Licensor provides the Work (and each Contributor provides its Contributions) on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied,
 * including, without limitation, any warranties or conditions of TITLE, NON-INFRINGEMENT,
 * MERCHANTABILITY, or FITNESS FOR A PARTICULAR PURPOSE.
 *
 * Whilst the Work is capable of being deployed, used and adopted (instantiated) to create
 * a distributed ledger it is your responsibility to test and validate the code, together
 * with all logic and performance of that code under all foreseeable scenarios.
 *
 * The Licensor does not make or purport to make and hereby excludes liability for all
 * and any representation, warranty or undertaking in any form whatsoever, whether express
 * or implied, to any entity or person, including any representation, warranty or
 * undertaking, as to the functionality security use, value or other characteristics of
 * any distributed ledger nor in respect the functioning or value of any tokens which may
 * be created stored or transferred using the Work. The Licensor does not warrant that the
 * Work or any use of the Work complies with any law or regulation in any territory where
 * it may be implemented or used or that it will be appropriate for any specific purpose.
 *
 * Neither the licensor nor any current or former employees, officers, directors, partners,
 * trustees, representatives, agents, advisors, contractors, or volunteers of the Licensor
 * shall be liable for any direct or indirect, special, incidental, consequential or other
 * losses of any kind, in tort, contract or otherwise (including but not limited to loss
 * of revenue, income or profits, or loss of use or data, or loss of reputation, or loss
 * of any economic or other opportunity of whatsoever nature or howsoever arising), arising
 * out of or in connection with (without limitation of any use, misuse, of any ledger system
 * or use made or its functionality or any performance or operation of any code or protocol
 * caused by bugs or programming or logic errors or otherwise);
 *
 * A. any offer, purchase, holding, use, sale, exchange or transmission of any
 * cryptographic keys, tokens or assets created, exchanged, stored or arising from any
 * interaction with the Work;
 *
 * B. any failure in a transmission or loss of any token or assets keys or other digital
 * artefacts due to errors in transmission;
 *
 * C. bugs, hacks, logic errors or faults in the Work or any communication;
 *
 * D. system software or apparatus including but not limited to losses caused by errors
 * in holding or transmitting tokens by any third-party;
 *
 * E. breaches or failure of security including hacker attacks, loss or disclosure of
 * password, loss of private key, unauthorised use or misuse of such passwords or keys;
 *
 * F. any losses including loss of anticipated savings or other benefits resulting from
 * use of the Work or any changes to the Work (however implemented).
 *
 * You are solely responsible for; testing, validating and evaluation of all operation
 * logic, functionality, security and appropriateness of using the Work for any commercial
 * or non-commercial purpose and for any reproduction or redistribution by You of the
 * Work. You assume all risks associated with Your use of the Work and the exercise of
 * permissions under this License.
 */

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RadixDlt.NetworkGateway.DataAggregator.Configuration;
using RadixDlt.NetworkGateway.DataAggregator.NodeServices.ApiReaders;
using RadixDlt.NetworkGateway.DataAggregatorRunner;
using RadixDlt.NetworkGateway.IntegrationTests.CoreApiStubs;
using RadixDlt.NetworkGateway.IntegrationTests.Data;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace RadixDlt.NetworkGateway.IntegrationTests;

public class TestDataAggregatorFactory
    : WebApplicationFactory<Program>, ICollectionFixture<Program>
{
    private readonly CoreApiStub _coreApiStub;

    private readonly string _databaseName;
    private readonly ITestOutputHelper _testConsole;

    private TestDataAggregatorFactory(CoreApiStub coreApiStub, string databaseName, ITestOutputHelper testConsole)
    {
        _coreApiStub = coreApiStub;
        _databaseName = databaseName;
        _testConsole = testConsole;

        CreateClient();
    }

    public static TestDataAggregatorFactory Create(CoreApiStub coreApiStub, string databaseName, ITestOutputHelper testConsole)
    {
        testConsole.WriteLine("Creating TestDataAggregatorFactory");
        return new TestDataAggregatorFactory(coreApiStub, databaseName, testConsole);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var dbConnectionString =
            $"Host=127.0.0.1:5432;Database={_databaseName};Username=db_dev_superuser;Password=db_dev_password;Include Error Detail=true";

        builder
            .ConfigureAppConfiguration(
                (context, config) =>
                {
                    // connection string
                    config.AddInMemoryCollection(new[]
                    {
                        new KeyValuePair<string, string>("ConnectionStrings:NetworkGatewayReadOnly", dbConnectionString),
                        new KeyValuePair<string, string>("ConnectionStrings:NetworkGatewayReadWrite", dbConnectionString),
                        new KeyValuePair<string, string>("ConnectionStrings:NetworkGatewayMigrations", dbConnectionString),
                    });

                    // logging
                    config.AddInMemoryCollection(new[]
                    {
                        new KeyValuePair<string, string>("Logging:LogLevel:Default", "Warning"),
                        new KeyValuePair<string, string>("Logging:LogLevel:Microsoft.AspNetCore", "Warning"),
                        new KeyValuePair<string, string>("Microsoft.Hosting.Lifetime", "Information"),
                        new KeyValuePair<string, string>("Microsoft.EntityFrameworkCore.Database.Command", "Warning"),
                        new KeyValuePair<string, string>("Microsoft.EntityFrameworkCore.Infrastructure", "Warning"),
                        new KeyValuePair<string, string>("System.Net.Http.HttpClient.ICoreApiProvider.LogicalHandler", "Warning"),
                        new KeyValuePair<string, string>("System.Net.Http.HttpClient.ICoreApiProvider.ClientHandler", "Warning"),
                    });

                    // mempool
                    config.AddInMemoryCollection(new[]
                    {
                        new KeyValuePair<string, string>(
                            "DataAggregator:Mempool:MinDelayBetweenMissingFromMempoolAndResubmissionSeconds", "10"),
                        new KeyValuePair<string, string>(
                            "DataAggregator:Mempool:MinDelayBetweenResubmissionsSeconds", "10"),
                        new KeyValuePair<string, string>("DataAggregator:Mempool:StopResubmittingAfterSeconds", "300"),
                        new KeyValuePair<string, string>("DataAggregator:Mempool:PruneCommittedAfterSeconds", "10"),
                        new KeyValuePair<string, string>(
                            "DataAggregator:Mempool:PruneMissingTransactionsAfterTimeSinceLastGatewaySubmissionSeconds", "604800"),
                        new KeyValuePair<string, string>(
                            "DataAggregator:Mempool:PruneMissingTransactionsAfterTimeSinceFirstSeenSeconds", "604800"),
                        new KeyValuePair<string, string>(
                            "DataAggregator:Mempool:PruneRequiresMissingFromMempoolForSeconds", "60"),
                    });

                    // ledgerConfirmation
                    config.AddInMemoryCollection(new[]
                    {
                        new KeyValuePair<string, string>(
                            "DataAggregator:LedgerConfirmation:CommitRequiresNodeQuorumTrustProportion", "0.51"),
                        new KeyValuePair<string, string>(
                            "DataAggregator:LedgerConfirmation:OnlyUseSufficientlySyncedUpNodesForQuorumCalculation",
                            "true"),
                        new KeyValuePair<string, string>(
                            "DataAggregator:LedgerConfirmation:SufficientlySyncedStateVersionThreshold", "1000"),
                        new KeyValuePair<string, string>("DataAggregator:LedgerConfirmation:MaxCommitBatchSize", "1000"),
                        new KeyValuePair<string, string>(
                            "DataAggregator:LedgerConfirmation:MaxTransactionPipelineSizePerNode", "3000"),
                        new KeyValuePair<string, string>(
                            "DataAggregator:LedgerConfirmation:LargeBatchSizeToAddDelay", "500"),
                        new KeyValuePair<string, string>(
                            "DataAggregator:LedgerConfirmation:DelayBetweenLargeBatchesMilliseconds", "0"),
                    });

                    // transactionAssertions
                    config.AddInMemoryCollection(new[]
                    {
                        new KeyValuePair<string, string>(
                            "DataAggregator:TransactionAssertions:AssertDownedSubstatesMatchDownFromCoreApi",
                            "false"),
                        new KeyValuePair<string, string>(
                            "DataAggregator:TransactionAssertions:SubstateTypesWhichAreAllowedToHaveIncompleteHistoryCommaSeparated",
                            "ValidatorSystemMetadataSubstate"),
                    });
                }
            )
            .ConfigureTestServices(services =>
            {
                _testConsole.WriteLine("Injecting core api stubs");

                // inject core stubs
                services.AddSingleton<INetworkConfigurationReader>(_coreApiStub);
                services.AddSingleton<ITransactionLogReader>(_coreApiStub);
                services.AddSingleton<ICoreApiProvider>(_coreApiStub);

                services.PostConfigure<NetworkOptions>(o =>
                    {
                        o.NetworkName = GenesisData.NetworkDefinition.LogicalName;
                        o.DisableCoreApiHttpsCertificateChecks = false;
                        o.CoreApiNodes = new List<CoreApiNode>
                        {
                            _coreApiStub.RequestsAndResponses.DataAggregatorCoreApiNode,
                        };
                    }
                );
            });
    }
}
