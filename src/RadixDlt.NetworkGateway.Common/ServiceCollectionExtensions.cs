using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace RadixDlt.NetworkGateway.Common;

public static class ServiceCollectionExtensions
{
    public static void AddNetworkGatewayCore(this IServiceCollection services)
    {
        services.TryAddSingleton<ISystemClock, TmpSystemClock>();
    }
}
