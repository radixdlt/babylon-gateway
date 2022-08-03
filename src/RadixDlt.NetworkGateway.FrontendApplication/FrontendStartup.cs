using RadixDlt.NetworkGateway.Frontend.Configuration;

namespace RadixDlt.NetworkGateway.FrontendApplication;

public class FrontendStartup
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
