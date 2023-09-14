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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using RadixDlt.NetworkGateway.Abstractions;
using RadixDlt.NetworkGateway.Abstractions.Configuration;
using RadixDlt.NetworkGateway.Abstractions.CoreCommunications;
using RadixDlt.NetworkGateway.DataAggregator.Configuration;
using RadixDlt.NetworkGateway.DataAggregator.Monitoring;
using RadixDlt.NetworkGateway.DataAggregator.NodeServices;
using RadixDlt.NetworkGateway.DataAggregator.NodeServices.ApiReaders;
using RadixDlt.NetworkGateway.DataAggregator.Services;
using RadixDlt.NetworkGateway.DataAggregator.Workers.NodeWorkers;
using System.Net;
using System.Net.Http;

namespace RadixDlt.NetworkGateway.DataAggregator;

public static class ServiceCollectionExtensions
{
    public static DataAggregatorBuilder AddNetworkGatewayDataAggregator(this IServiceCollection services)
    {
        var builder = services
            .AddNetworkGatewayDataAggregatorCore()
            .AddHealthChecks()
            .AddHostedServices();

        return builder;
    }

    public static DataAggregatorBuilder AddNetworkGatewayDataAggregatorCore(this IServiceCollection services)
    {
        services
            .AddNetworkGatewayAbstractions();

        services
            .AddValidatableOptionsAtSection<NetworkOptions, NetworkOptionsValidator>("DataAggregator:Network")
            .AddValidatableOptionsAtSection<MonitoringOptions, MonitoringOptionsValidator>("DataAggregator:Monitoring")
            .AddValidatableOptionsAtSection<MempoolOptions, MempoolOptionsValidator>("DataAggregator:Mempool")
            .AddValidatableOptionsAtSection<LedgerConfirmationOptions, LedgerConfirmationOptionsValidator>("DataAggregator:LedgerConfirmation")
            .AddValidatableOptionsAtSection<TransactionAssertionsOptions, TransactionAssertionsOptionsValidator>("DataAggregator:TransactionAssertions");

        // Globally-Scoped services
        AddGlobalScopedServices(services);

        // Node-Scoped services
        AddNodeScopedServices(services);
        AddTransientApiReaders(services, out var coreApiHttpClientBuilder);
        AddNodeInitializers(services);
        AddNodeWorkers(services);

        return new DataAggregatorBuilder(services, coreApiHttpClientBuilder);
    }

    private static void AddGlobalScopedServices(IServiceCollection services)
    {
        services.TryAddSingleton<INodeWorkersRunnerRegistry, NodeWorkersRunnerRegistry>();
        services.TryAddSingleton<INodeWorkersRunnerFactory, NodeWorkersRunnerFactory>();
        services.TryAddSingleton<ILedgerTransactionsProcessor, LedgerTransactionsProcessor>();
        services.TryAddSingleton<INetworkAddressConfigProvider>(x => x.GetRequiredService<INetworkConfigurationProvider>());
        services.TryAddSingleton<ISystemStatusService, SystemStatusService>();
    }

    private static void AddNodeScopedServices(IServiceCollection services)
    {
        services.TryAddScoped<INodeConfigProvider, NodeConfigProvider>();
    }

    private static void AddTransientApiReaders(IServiceCollection services, out IHttpClientBuilder coreApiHttpClientBuilder)
    {
        // NB - AddHttpClient is essentially like AddTransient, except it provides a HttpClient from the HttpClientFactory
        // See https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests
        coreApiHttpClientBuilder = services
            .AddHttpClient<ICoreApiProvider, CoreApiProvider>()
            .ConfigurePrimaryHttpMessageHandler(serviceProvider => ConfigureHttpClientHandler(serviceProvider.GetRequiredService<IOptions<NetworkOptions>>()));

        // We can mock these out in tests
        // These should be transient so that they don't capture a transient HttpClient
        services.TryAddTransient<ITransactionStreamReader, TransactionStreamReader>();
        services.TryAddTransient<INetworkConfigurationReader, NetworkConfigurationReader>();
        services.TryAddTransient<INetworkStatusReader, NetworkStatusReader>();
    }

    private static void AddNodeInitializers(IServiceCollection services)
    {
        // Add node initializers - these will be instantiated by the NodeWorkersRunner.cs and run before the workers start
        services.TryAddScoped<INodeInitializer, NodeNetworkConfigurationInitializer>();
    }

    private static void AddNodeWorkers(IServiceCollection services)
    {
        // Add node workers - these will be instantiated by the NodeWorkersRunner.cs.
        services.TryAddScoped<INodeWorker, NodeTransactionFetcherWorker>();
        services.AddScoped<INodeWorker, NodeMempoolTransactionHashesReaderWorker>();
        services.AddScoped<INodeWorker, NodeMempoolFullTransactionReaderWorker>();
    }

    private static HttpClientHandler ConfigureHttpClientHandler(IOptions<NetworkOptions> options)
    {
        var o = options.Value;
        var httpClientHandler = new HttpClientHandler();

        if (o.DisableCoreApiHttpsCertificateChecks)
        {
            httpClientHandler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
        }

        if (!string.IsNullOrWhiteSpace(o.CoreApiHttpProxyAddress))
        {
            httpClientHandler.Proxy = new WebProxy(o.CoreApiHttpProxyAddress);
        }

        // Enables gzip,deflate,brotli for Core API requests
        // See https://learn.microsoft.com/en-us/dotnet/api/system.net.http.httpclienthandler.automaticdecompression?view=net-7.0
        httpClientHandler.AutomaticDecompression = DecompressionMethods.All;

        return httpClientHandler;
    }
}
