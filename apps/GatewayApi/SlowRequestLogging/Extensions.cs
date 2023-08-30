using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace GatewayApi.SlowRequestLogging;

public static class Extensions
{
    public static IServiceCollection AddSlowRequestLogging(this IServiceCollection services, Action<SlowRequestLoggingOptions> configureOptions)
    {
        return services.Configure(configureOptions);
    }

    public static IApplicationBuilder UseSlowRequestLogging(this IApplicationBuilder app)
    {
        return app.UseMiddleware<SlowRequestLoggingMiddleware>();
    }
}
