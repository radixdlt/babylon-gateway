using Microsoft.AspNetCore.Builder;
using RadixDlt.NetworkGateway.GatewayApi.AspNetCore;

namespace RadixDlt.NetworkGateway.GatewayApi;

public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Adds a <see cref="RequestTimeoutMiddleware"/> middleware to the specified <see cref="IApplicationBuilder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IApplicationBuilder"/> to add the middleware to.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static IApplicationBuilder UseRequestTimeout(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestTimeoutMiddleware>();
    }
}
