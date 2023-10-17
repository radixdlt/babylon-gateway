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

using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using RadixDlt.NetworkGateway.Abstractions;
using RadixDlt.NetworkGateway.Abstractions.Configuration;
using RadixDlt.NetworkGateway.Abstractions.CoreCommunications;
using RadixDlt.NetworkGateway.GatewayApi.AspNetCore;
using RadixDlt.NetworkGateway.GatewayApi.Configuration;
using RadixDlt.NetworkGateway.GatewayApi.CoreCommunications;
using RadixDlt.NetworkGateway.GatewayApi.Handlers;
using RadixDlt.NetworkGateway.GatewayApi.Services;
using System.Net;
using System.Net.Http;

namespace RadixDlt.NetworkGateway.GatewayApi;

public static class ServiceCollectionExtensions
{
    public static GatewayApiBuilder AddNetworkGatewayApi(this IServiceCollection services)
    {
        var builder = services
            .AddNetworkGatewayApiCore()
            .AddWorkerServices()
            .AddHostedServices();

        return builder;
    }

    public static GatewayApiBuilder AddNetworkGatewayApiCore(this IServiceCollection services)
    {
        services
            .AddNetworkGatewayAbstractions();

        services
            .AddValidatableOptionsAtSection<EndpointOptions, EndpointOptionsValidator>("GatewayApi:Endpoint")
            .AddValidatableOptionsAtSection<SlowQueryLoggingOptions, SlowQueryLoggingOptionsValidator>("GatewayApi:SlowQueryLogging")
            .AddValidatableOptionsAtSection<CoreApiIntegrationOptions, CoreApiIntegrationOptionsValidator>("GatewayApi:CoreApiIntegration")
            .AddValidatableOptionsAtSection<NetworkOptions, NetworkOptionsValidator>("GatewayApi:Network")
            .AddValidatableOptionsAtSection<AcceptableLedgerLagOptions, AcceptableLedgerLagOptionsValidator>("GatewayApi:AcceptableLedgerLag");

        services
            .AddValidatorsFromAssemblyContaining(typeof(ServiceCollectionExtensions), includeInternalTypes: true)
            .AddFluentValidationAutoValidation(o =>
            {
                // our own validators replicate all the basic rules defined by System.ComponentModel.DataAnnotations
                // in order to avoid duplicated validation messages we disable this built-in mechanism
                o.DisableDataAnnotationsValidation = true;
            });

        // Singleton-Scoped services
        AddSingletonServices(services);

        // Request-scoped services
        AddRequestServices(services);

        // Transient (pooled) services
        AddCoreApiHttpClient(services, out var coreApiHttpClientBuilder, out var coreNodeHealthCheckerClientBuilder);

        return new GatewayApiBuilder(services, coreApiHttpClientBuilder, coreNodeHealthCheckerClientBuilder);
    }

    private static void AddSingletonServices(IServiceCollection services)
    {
        // Should only contain services without any DBContext or HttpClient - as these both need to be recycled
        // semi-regularly
        services.TryAddSingleton<INetworkConfigurationProvider, NetworkConfigurationProvider>();
        services.TryAddSingleton<INetworkAddressConfigProvider>(x => x.GetRequiredService<INetworkConfigurationProvider>());
        services.TryAddSingleton<IValidationErrorHandler, ValidationErrorHandler>();
        services.TryAddSingleton<ICoreNodesSelectorService, CoreNodesSelectorService>();
        services.TryAddSingleton<RequestTimeoutMiddleware>();
    }

    private static void AddRequestServices(IServiceCollection services)
    {
        services.TryAddScoped<IEntityHandler, DefaultEntityHandler>();
        services.TryAddScoped<IValidatorHandler, DefaultValidatorHandler>();
        services.TryAddScoped<IStatusHandler, DefaultStatusHandler>();
        services.TryAddScoped<ITransactionHandler, DefaultTransactionHandler>();
        services.TryAddScoped<IValidatorStateHandler, DefaultValidatorStateHandler>();
        services.TryAddScoped<INonFungibleHandler, DefaultNonFungibleHandler>();
        services.TryAddScoped<IKeyValueStoreHandler, DefaultKeyValueStoreHandler>();
        services.TryAddScoped<ITransactionPreviewService, TransactionPreviewService>();
        services.TryAddScoped<ITransactionBalanceChangesService, TransactionBalanceChangesService>();
        services.TryAddScoped<ISubmissionService, SubmissionService>();
    }

    private static void AddCoreApiHttpClient(IServiceCollection services, out IHttpClientBuilder coreApiHttpClientBuilder, out IHttpClientBuilder coreNodeHealthCheckerClientBuilder)
    {
        // NB - AddHttpClient is essentially like AddTransient, except it provides a HttpClient from the HttpClientFactory
        // See https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests
        coreApiHttpClientBuilder = services
            .AddHttpClient<ICoreApiHandler, CoreApiHandler>()
            .ConfigurePrimaryHttpMessageHandler(serviceProvider => ConfigureHttpClientHandler(serviceProvider.GetRequiredService<IOptions<NetworkOptions>>()));

        coreNodeHealthCheckerClientBuilder = services
            .AddHttpClient<ICoreNodeHealthChecker, CoreNodeHealthChecker>()
            .ConfigurePrimaryHttpMessageHandler(serviceProvider => ConfigureHttpClientHandler(serviceProvider.GetRequiredService<IOptions<NetworkOptions>>()));
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
