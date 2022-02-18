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

// StyleCop getting confused with flat Program.cs
#pragma warning disable SA1516

using Common.Database;
using DataAggregator.Configuration;
using DataAggregator.DependencyInjection;
using DataAggregator.GlobalServices;
using DataAggregator.Monitoring;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);
var hostBuilder = builder.Host;

IConfigurationRoot? bootUpConfiguration;

hostBuilder.ConfigureAppConfiguration((context, config) =>
{
    config.AddEnvironmentVariables("RADIX_NG_AGGREGATOR__");
    config.AddEnvironmentVariables("RADIX_NG_AGGREGATOR:"); // Remove this line once https://github.com/dotnet/runtime/issues/61577#issuecomment-1044959384 is fixed
    if (args is { Length: > 0 })
    {
        config.AddCommandLine(args);
    }

    if (context.HostingEnvironment.IsDevelopment())
    {
        // As an easier alternative to developer secrets -- this file is in .gitignore to prevent source controlling
        config.AddJsonFile("appsettings.PersonalOverrides.json", optional: true, reloadOnChange: true);
    }
    else
    {
        config.AddJsonFile("appsettings.ProductionOverrides.json", optional: true, reloadOnChange: true);
    }

    bootUpConfiguration = config.Build();
    var customConfigurationPath = bootUpConfiguration
        .GetValue<string?>("CustomJsonConfigurationFilePath", null);

    if (customConfigurationPath != null)
    {
        config.AddJsonFile(customConfigurationPath, false, true);
        bootUpConfiguration = config.Build();
    }
});

hostBuilder.ConfigureServices(new DefaultKernel().ConfigureServices);
hostBuilder.ConfigureLogging((_, loggingBuilder) =>
{
    loggingBuilder.AddConsole();
});

var servicesBuilder = builder.Services;

servicesBuilder.AddControllers();

servicesBuilder.AddHealthChecks()
    .AddCheck<AggregatorHealthCheck>("aggregator_health_check")
    .AddDbContextCheck<AggregatorDbContext>("database_connection_check")
    .ForwardToPrometheus();

var app = builder.Build();
var services = app.Services;

var configuration = services.GetRequiredService<IConfiguration>();
var aggregatorConfiguration = services.GetRequiredService<IAggregatorConfiguration>();
var programLogger = services.GetRequiredService<ILogger<Program>>();

// https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks?view=aspnetcore-6.0
app.MapHealthChecks("/health");

var shouldWipeDatabaseInsteadOfStart = configuration.GetValue<bool>("WIPE_DATABASE");

var maxWaitForDbMs = configuration.GetValue("MaxWaitForDbOnStartupMs", 5000);

if (shouldWipeDatabaseInsteadOfStart)
{
    await ConnectionHelpers.PerformScopedDbAction<AggregatorDbContext>(services, async (_, logger, dbContext) =>
    {
        logger.LogInformation("Connecting to database - if it exists");

        await ConnectionHelpers.TryWaitForExistingDbConnection(logger, dbContext, maxWaitForDbMs);

        logger.LogInformation("Starting database wipe");

        await dbContext.Database.EnsureDeletedAsync();

        logger.LogInformation("Database wipe completed. Now stopping...");
    });

    // Stop the program
    return;
}

// TODO:NG-14 - Change to manage migrations more safely outside service boot-up
// TODO:NG-38 - Tweak logs so that any migration based logs still appear, but that general Microsoft.EntityFrameworkCore.Database.Command logs do not
await ConnectionHelpers.PerformScopedDbAction<AggregatorDbContext>(services, async (scope, logger, dbContext) =>
{
    logger.LogInformation("Starting database migrations if required");

    await ConnectionHelpers.MigrateWithRetry(logger, dbContext, maxWaitForDbMs);

    logger.LogInformation("Database migrations (if required) were completed");

    var networkConfigurationService = scope.ServiceProvider.GetRequiredService<INetworkConfigurationProvider>();

    var existingNetworkName = await networkConfigurationService
        .EnsureNetworkConfigurationLoadedFromDatabaseIfExistsAndReturnNetworkName();

    if (existingNetworkName != null && existingNetworkName != aggregatorConfiguration.GetNetworkName())
    {
        throw new Exception(
            $"Aggregator was started up with network name {aggregatorConfiguration.GetNetworkName()} but the database has an existing ledger with network name {existingNetworkName}"
        );
    }
});

app.MapControllers(); // Root controllers - mapped to port 80 by default in prod
StartMetricServer();

await app.RunAsync(); // Health and Root controller on default port

/* Methods */

void StartMetricServer()
{
    var metricPort = configuration.GetValue<int>("PrometheusMetricsPort");
    if (metricPort != 0)
    {
        programLogger.LogInformation("Starting metrics server on port http://localhost:{MetricPort}", metricPort);
        new KestrelMetricServer(port: metricPort).Start();
    }
    else
    {
        programLogger.LogInformation("PrometheusMetricsPort not configured - not starting metric server");
    }
}
