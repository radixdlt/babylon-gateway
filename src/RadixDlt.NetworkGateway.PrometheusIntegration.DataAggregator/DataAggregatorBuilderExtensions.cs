using Microsoft.Extensions.DependencyInjection;
using Prometheus;
using RadixDlt.NetworkGateway.DataAggregator;
using RadixDlt.NetworkGateway.DataAggregator.Monitoring;
using RadixDlt.NetworkGateway.DataAggregator.NodeServices;
using RadixDlt.NetworkGateway.DataAggregator.NodeServices.ApiReaders;
using RadixDlt.NetworkGateway.DataAggregator.Services;
using RadixDlt.NetworkGateway.DataAggregator.Workers.GlobalWorkers;
using RadixDlt.NetworkGateway.DataAggregator.Workers.NodeWorkers;

namespace RadixDlt.NetworkGateway.PrometheusIntegration.DataAggregator;

public static class DataAggregatorBuilderExtensions
{
    public static DataAggregatorBuilder UsePrometheusMetrics(this DataAggregatorBuilder builder)
    {
        builder.Services
            .AddSingleton<MetricsObserver>()
            .AddSingleton<IGlobalWorkerObserver>(provider => provider.GetRequiredService<MetricsObserver>())
            .AddSingleton<INodeWorkerObserver>(provider => provider.GetRequiredService<MetricsObserver>())
            .AddSingleton<ILedgerConfirmationServiceObserver>(provider => provider.GetRequiredService<MetricsObserver>())
            .AddSingleton<IMempoolPrunerServiceObserver>(provider => provider.GetRequiredService<MetricsObserver>())
            .AddSingleton<IMempoolResubmissionServiceObserver>(provider => provider.GetRequiredService<MetricsObserver>())
            .AddSingleton<IAggregatorHealthCheckObserver>(provider => provider.GetRequiredService<MetricsObserver>())
            .AddSingleton<ISystemStatusServiceObserver>(provider => provider.GetRequiredService<MetricsObserver>())
            .AddSingleton<INodeInitializerObserver>(provider => provider.GetRequiredService<MetricsObserver>())
            .AddSingleton<IMempoolTrackerServiceObserver>(provider => provider.GetRequiredService<MetricsObserver>())
            .AddSingleton<INodeTransactionLogWorkerObserver>(provider => provider.GetRequiredService<MetricsObserver>())
            .AddSingleton<INodeMempoolTransactionIdsReaderWorkerObserver>(provider => provider.GetRequiredService<MetricsObserver>())
            .AddSingleton<INodeMempoolFullTransactionReaderWorkerObserver>(provider => provider.GetRequiredService<MetricsObserver>())
            .AddSingleton<IRawTransactionWriterObserver>(provider => provider.GetRequiredService<MetricsObserver>())
            .AddSingleton<INetworkConfigurationReaderObserver>(provider => provider.GetRequiredService<MetricsObserver>())
            .AddSingleton<INetworkStatusReaderObserver>(provider => provider.GetRequiredService<MetricsObserver>())
            .AddSingleton<ITransactionLogReaderObserver>(provider => provider.GetRequiredService<MetricsObserver>());

        builder.CoreApiHttpClientBuilder
            .UseHttpClientMetrics();

        return builder;
    }
}
