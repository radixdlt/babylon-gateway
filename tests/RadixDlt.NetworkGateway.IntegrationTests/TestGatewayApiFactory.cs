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
using Microsoft.Extensions.Logging;
using RadixDlt.CoreApiSdk.Model;
using RadixDlt.NetworkGateway.Commons.Addressing;
using RadixDlt.NetworkGateway.Commons.Extensions;
using RadixDlt.NetworkGateway.Commons.Model;
using RadixDlt.NetworkGateway.DataAggregator.NodeServices.ApiReaders;
using RadixDlt.NetworkGateway.DataAggregator.Services;
using RadixDlt.NetworkGateway.GatewayApi.Configuration;
using RadixDlt.NetworkGateway.GatewayApi.CoreCommunications;
using RadixDlt.NetworkGateway.GatewayApi.Services;
using RadixDlt.NetworkGateway.GatewayApiTestServer;
using RadixDlt.NetworkGateway.IntegrationTests.CoreApiStubs;
using RadixDlt.NetworkGateway.IntegrationTests.Utilities;
using RadixDlt.NetworkGateway.PostgresIntegration;
using RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Xunit;

namespace RadixDlt.NetworkGateway.IntegrationTests
{
    public class TestGatewayApiFactory
        : WebApplicationFactory<TestGatewayApiStartup>, ICollectionFixture<TestGatewayApiFactory>
    {
        private readonly CoreApiStub _coreApiStub;

        private readonly string _databaseName;

        public HttpClient Client { get; }

        private TestGatewayApiFactory(CoreApiStub coreApiStub, string databaseName)
        {
            _coreApiStub = coreApiStub;
            _databaseName = databaseName;

            Client = CreateClient();
        }

        public static TestGatewayApiFactory Create(CoreApiStub coreApiStub, string databaseName)
        {
            return new TestGatewayApiFactory(coreApiStub, databaseName);
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            var dbConnectionString = $"Host=127.0.0.1:5432;Database={_databaseName};Username=db_dev_superuser;Password=db_dev_password;Include Error Detail=true";

            builder
            .ConfigureAppConfiguration(
                    (_, config) =>
                    {
                        config.AddInMemoryCollection(new[]
                        {
                            new KeyValuePair<string, string>("ConnectionStrings:NetworkGatewayReadOnly", dbConnectionString),
                            new KeyValuePair<string, string>("ConnectionStrings:NetworkGatewayReadWrite", dbConnectionString),
                            new KeyValuePair<string, string>("ConnectionStrings:NetworkGatewayMigrations", dbConnectionString),
                        });
                    }
            )
            .ConfigureTestServices(services =>
            {
                // // inject core mocks
                // foreach (Type mockType in _coreApiMocks.Keys)
                // {
                //     services.AddSingleton(mockType, _coreApiMocks[mockType]!);
                // }

                // inject core stubs
                services.AddSingleton<ICoreNodeHealthChecker>(_coreApiStub);
                services.AddSingleton<INetworkConfigurationReader>(_coreApiStub);
                services.AddSingleton<ITransactionLogReader>(_coreApiStub);
                // services.AddSingleton<ICoreApiProvider>(_coreApiStub);
                services.AddSingleton<ICoreApiHandler>(_coreApiStub);

                var sp = services.BuildServiceProvider();

                using var scope = sp.CreateScope();

                var scopedServices = scope.ServiceProvider;

                var logger = scopedServices
                    .GetRequiredService<ILogger<TestGatewayApiFactory>>();

                var dbReadyOnlyContext = scopedServices.GetRequiredService<ReadOnlyDbContext>();

                dbReadyOnlyContext.Database.EnsureDeleted();

                // This function will also run migrations!
                dbReadyOnlyContext.Database.EnsureCreated();

                try
                {
                    InitializeDbForTests(dbReadyOnlyContext);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"An error occurred when initializing the database for tests. Error: {ex.Message}");
                }

                services.PostConfigure<NetworkOptions>(o =>
                    {
                        o.NetworkName = _coreApiStub.CoreApiStubDefaultConfiguration.NetworkName;
                        o.IgnoreNonSyncedNodes = false;
                        o.CoreApiNodes = new List<CoreApiNode>()
                        {
                            _coreApiStub.CoreApiStubDefaultConfiguration.GatewayCoreApiNode,
                        };
                    }
                );

                services.PostConfigure<EndpointOptions>(o =>
                {
                    o.GatewayApiVersion = "3.0.0";
                    o.GatewayOpenApiSchemaVersion = "2.0.0";
                    o.MaxPageSize = 30;
                });
            });
        }

        private void InitializeDbForTests(ReadOnlyDbContext db)
        {
            // network configuration
            db.NetworkConfiguration.Add(MapNetworkConfigurationResponse(_coreApiStub.CoreApiStubDefaultConfiguration
                .NetworkConfigurationResponse));

            // ledger and raw transaction
            db.RawTransactions.Add(new RawTransaction()
            {
                Payload = _coreApiStub.CoreApiStubDefaultConfiguration.Hash.ConvertFromHex(),
                TransactionPayloadHash = _coreApiStub.CoreApiStubDefaultConfiguration.Hash.ConvertFromHex(),
            });

            // mempool transactions
            var mempoolTransaction = MempoolTransaction.NewAsSubmittedForFirstTimeByGateway(
                payloadHash: _coreApiStub.CoreApiStubDefaultConfiguration.MempoolTransactionHash.ConvertFromHex(),
                payload: Encoding.UTF8.GetBytes(_coreApiStub.CoreApiStubDefaultConfiguration.SubmitTransaction),
                submittedToNodeName: _coreApiStub.CoreApiStubDefaultConfiguration.GatewayCoreApiNode.Name,
                transactionContents: GatewayTransactionContents.Default(),
                submittedTimestamp: new FakeClock().UtcNow
            );

            db.MempoolTransactions.Add(mempoolTransaction);

            // set status
            switch (_coreApiStub.CoreApiStubDefaultConfiguration.MempoolTransaction.TransactionStatus)
            {
                case MempoolTransactionStatus.Committed:
                    mempoolTransaction.MarkAsCommitted(
                        _coreApiStub.CoreApiStubDefaultConfiguration.TransactionSummary.StateVersion,
                        new FakeClock().UtcNow, new FakeClock());
                    break;
                case MempoolTransactionStatus.Failed:
                    mempoolTransaction.MarkAsFailed(
                        failureReason: MempoolTransactionFailureReason.Timeout,
                        failureExplanation: "stack snapshot",
                        timestamp: new FakeClock().UtcNow);
                    break;
                case MempoolTransactionStatus.Missing:
                    break;
                case MempoolTransactionStatus.ResolvedButUnknownTillSyncedUp:
                    break;
                case MempoolTransactionStatus.SubmittedOrKnownInNodeMempool:
                    mempoolTransaction.MarkAsSubmittedToGateway(new FakeClock().UtcNow);
                    break;
            }

            db.LedgerTransactions.Add(TransactionMapping.CreateLedgerTransaction(
                    new CommittedTransactionData(
                        _coreApiStub.CoreApiStubDefaultConfiguration.CommittedTransaction,
                        _coreApiStub.CoreApiStubDefaultConfiguration.TransactionSummary,
                        _coreApiStub.CoreApiStubDefaultConfiguration.CommittedTransaction.Metadata.Hex.ConvertFromHex())
                )
            );

            // ledger status
            db.LedgerStatus.Add(new LedgerStatus()
            {
                LastUpdated = new FakeClock().UtcNow,
                TopOfLedgerStateVersion = _coreApiStub.CoreApiStubDefaultConfiguration.TransactionSummary.StateVersion,
                SyncTarget = new SyncTarget() { TargetStateVersion = 1 },
            });

            db.SaveChanges();
        }

        private NetworkConfiguration MapNetworkConfigurationResponse(NetworkConfigurationResponse networkConfiguration)
        {
            var hrps = networkConfiguration.Bech32HumanReadableParts;
            return new NetworkConfiguration
            {
                NetworkDefinition = new NetworkDefinition { NetworkName = networkConfiguration.NetworkIdentifier.Network },
                NetworkAddressHrps = new NetworkAddressHrps
                {
                    AccountHrp = hrps.AccountHrp,
                    ResourceHrpSuffix = hrps.ResourceHrpSuffix,
                    ValidatorHrp = hrps.ValidatorHrp,
                    NodeHrp = hrps.NodeHrp,
                },
                WellKnownAddresses = new WellKnownAddresses
                {
                    XrdAddress = RadixBech32.GenerateXrdAddress(hrps.ResourceHrpSuffix),
                },
            };
        }
    }
}
