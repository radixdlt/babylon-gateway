using GatewayApi;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RadixDlt.NetworkGateway.Core.Database;
using RadixDlt.NetworkGateway.GatewayApi;

namespace RadixDlt.NetworkGateway.IntegrationTests.GatewayApi
{
    public class TestApplicationFactory : WebApplicationFactory<GatewayApiStartup>
    {
        //private readonly string _environment;
        private IConfiguration configuration;

        public TestApplicationFactory()
        {
            //_environment = "Development";
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType ==
                        typeof(DbContextOptions<ReadOnlyDbContext>));

                services.Remove(descriptor);

                services.AddDbContext<ReadOnlyDbContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryReadOnlyDb");
                });

                var sp = services.BuildServiceProvider();

                using (var scope = sp.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<ReadOnlyDbContext>();
                    var logger = scopedServices
                        .GetRequiredService<ILogger<TestApplicationFactory>>();

                    db.Database.EnsureCreated();

                    try
                    {
                        InMemoryDb.InitializeDbForTests(db);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "An error occurred seeding the " +
                            "database with seed data. Error: {Message}", ex.Message);
                    }
                }
            });
        }

        protected override IWebHostBuilder CreateWebHostBuilder()
        {
            var hostBuilder = new WebHostBuilder()
            .UseContentRoot(Directory.GetCurrentDirectory())
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.AddJsonFile(@"appsettings.Development.json", optional: false, reloadOnChange: true);

                config.AddEnvironmentVariables();
            })
            .UseStartup<GatewayApiStartup>();

            return hostBuilder;
        }
        
        //protected override IHost CreateHost(IHostBuilder builder)
        //{
        //    builder.UseEnvironment(_environment);
        //    //var host = base.CreateHost(builder);

        //    var host = Host.CreateDefaultBuilder()
        //        .ConfigureServices((hostContext, services) =>
        //        {
        //            services.AddNetworkGatewayApi();
        //            services.AddControllers();
        //            services.AddOptions();
        //        })

        //        .ConfigureAppConfiguration((context, config) =>
        //        {
        //            config.AddEnvironmentVariables("APP__");
        //            config.AddEnvironmentVariables("APP:"); // Remove this line once https://github.com/dotnet/runtime/issues/61577#issuecomment-1044959384 is fixed

        //            if (context.HostingEnvironment.IsDevelopment())
        //            {
        //                // As an easier alternative to developer secrets -- this file is in .gitignore to prevent source controlling
        //                config.AddJsonFile("appsettings.DevelopmentOverrides.json", optional: true, reloadOnChange: true);
        //            }
        //            else
        //            {
        //                config.AddJsonFile("appsettings.ProductionOverrides.json", optional: true, reloadOnChange: true);
        //            }
        //        })
        //        .Build();

        //    return host;
        //}

        //public void ConfigureHost()
        //{
        //    new HostBuilder()
        //    .ConfigureWebHostDefaults(builder =>
        //    {
        //        // Use shared startup from your web app
        //        builder.UseStartup<Startup>();

        //        builder.UseTestServer();
        //        builder.ConfigureServices(services =>
        //        {
        //            services.AddSingleton(sp => sp.GetRequiredService<IHost>()
        //                .GetTestClient()
        //            );
        //        });
        //    });
        //}
    }
}
