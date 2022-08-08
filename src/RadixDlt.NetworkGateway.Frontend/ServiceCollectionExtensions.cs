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

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Prometheus;
using RadixDlt.NetworkGateway.Configuration;
using RadixDlt.NetworkGateway.CoreCommunications;
using RadixDlt.NetworkGateway.Extensions;
using RadixDlt.NetworkGateway.Frontend.Configuration;
using RadixDlt.NetworkGateway.Frontend.CoreCommunications;
using RadixDlt.NetworkGateway.Frontend.Endpoints;
using RadixDlt.NetworkGateway.Frontend.Initializers;
using RadixDlt.NetworkGateway.Frontend.Services;
using RadixDlt.NetworkGateway.Frontend.Workers;
using System.Net;

namespace RadixDlt.NetworkGateway.Frontend;

public static class ServiceCollectionExtensions
{
    public static void AddNetworkGatewayFrontend(this IServiceCollection services, string roConnectionString, string rwConnectionString)
    {
        services
            .AddValidatableOptionsAtSection<EndpointOptions, EndpointOptionsValidator>("GatewayApi:Endpoint")
            .AddValidatableOptionsAtSection<NetworkOptions, NetworkOptionsValidator>("GatewayApi:Network")
            .AddValidatableOptionsAtSection<AcceptableLedgerLagOptions, AcceptableLedgerLagOptionsValidator>("GatewayApi:AcceptableLedgerLag");

        services
            .AddOptions<NetworkOptions>()
            .ValidateDataAnnotations()
            .ValidateOnStart()
            .BindConfiguration("Api"); // TODO is this how we want to Bind configuration by default?

        // Initializers
        AddInitializers(services);

        // Singleton-Scoped services
        AddSingletonServices(services);
        AddHostedServices(services);

        // Request scoped services
        AddRequestScopedServices(services);
        AddReadOnlyDatabaseContext(services, roConnectionString);
        AddReadWriteDatabaseContext(services, rwConnectionString);

        // Other scoped services
        AddWorkerScopedServices(services);

        // Transient (pooled) services
        AddCoreApiHttpClient(services);
    }

    private static void AddInitializers(IServiceCollection services)
    {
        services
            .AddHostedService<NetworkConfigurationInitializer>();
    }

    private static void AddSingletonServices(IServiceCollection services)
    {
        // Should only contain services without any DBContext or HttpClient - as these both need to be recycled
        // semi-regularly
        services.AddSingleton<INetworkConfigurationProvider, NetworkConfigurationProvider>();
        services.AddSingleton<INetworkAddressConfigProvider>(x => x.GetRequiredService<INetworkConfigurationProvider>());
        services.AddSingleton<IValidations, Validations>();
        services.AddSingleton<IExceptionHandler, ExceptionHandler>();
        services.AddSingleton<IValidationErrorHandler, ValidationErrorHandler>();
        services.AddSingleton<IEntityDeterminer, EntityDeterminer>();
        services.AddSingleton<ICoreNodesSelectorService, CoreNodesSelectorService>();
    }

    private static void AddHostedServices(IServiceCollection services)
    {
        services.AddHostedService<CoreNodesSupervisorStatusReviseWorker>();
    }

    private static void AddRequestScopedServices(IServiceCollection services)
    {
        services.AddScoped<ILedgerStateQuerier, LedgerStateQuerier>();
        services.AddScoped<ITransactionQuerier, TransactionQuerier>();
        services.AddScoped<IConstructionAndSubmissionService, ConstructionAndSubmissionService>();
        services.AddScoped<ISubmissionTrackingService, SubmissionTrackingService>();
    }

    private static void AddWorkerScopedServices(IServiceCollection services)
    {
        services.AddScoped<ICoreNodeHealthChecker, CoreNodeHealthChecker>();
    }

    private static void AddCoreApiHttpClient(IServiceCollection services)
    {
        // NB - AddHttpClient is essentially like AddTransient, except it provides a HttpClient from the HttpClientFactory
        // See https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests
        services
            .AddHttpClient<ICoreApiHandler, CoreApiHandler>()
            .UseHttpClientMetrics()
            .ConfigurePrimaryHttpMessageHandler(serviceProvider => ConfigureHttpClientHandler(serviceProvider.GetRequiredService<IOptions<NetworkOptions>>()));

        services
            .AddHttpClient<ICoreNodeHealthChecker, CoreNodeHealthChecker>()
            .UseHttpClientMetrics()
            .ConfigurePrimaryHttpMessageHandler(serviceProvider => ConfigureHttpClientHandler(serviceProvider.GetRequiredService<IOptions<NetworkOptions>>()));
    }

    private static void AddReadOnlyDatabaseContext(IServiceCollection services, string roConnectionString)
    {
        services.AddDbContext<GatewayReadOnlyDbContext>(options =>
        {
            // https://www.npgsql.org/efcore/index.html
            options.UseNpgsql(roConnectionString, o => o.NonBrokenUseNodaTime());
        });
    }

    private static void AddReadWriteDatabaseContext(IServiceCollection services, string rwConnectionString)
    {
        services.AddDbContext<GatewayReadWriteDbContext>(options =>
        {
            // https://www.npgsql.org/efcore/index.html
            options.UseNpgsql(rwConnectionString, o => o.NonBrokenUseNodaTime());
        });
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

        return httpClientHandler;
    }
}
