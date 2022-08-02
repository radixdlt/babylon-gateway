using RadixDlt.NetworkGateway.Configuration;

namespace RadixDlt.NetworkGateway.Api;

public class ApiStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services
            .AddNetworkGatewayApi();

        services
            .AddCors(options =>
            {
                options.AddDefaultPolicy(corsPolicyBuilder =>
                {
                    corsPolicyBuilder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                });
            });

        services
            .AddControllers()
            .AddNewtonsoftJson();

        services
            .AddHealthChecks();
    }

    public void Configure(IApplicationBuilder application)
    {
        application
            .UseAuthentication()
            .UseAuthorization()
            .UseCors()
            .UseRouting()
            .UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health");
                endpoints.MapNetworkGatewayApi();
                endpoints.MapControllers();
            });
    }
}
