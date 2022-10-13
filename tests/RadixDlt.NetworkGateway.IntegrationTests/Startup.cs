using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RadixDlt.NetworkGateway.IntegrationTests.Data;
using System;
using System.IO;

namespace RadixDlt.NetworkGateway.IntegrationTests;

public class TestSetup
{
    public TestSetup()
    {
        var serviceCollection = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(
                path: "appsettings.json",
                optional: false,
                reloadOnChange: true)
            .Build();
        serviceCollection.AddSingleton<IConfiguration>(configuration);

        ServiceProvider = serviceCollection.BuildServiceProvider();

        GenesisData.NetworkDefinition = GetNetworkDefinition();
    }

    public ServiceProvider ServiceProvider { get; private set; }

    private NetworkDefinition GetNetworkDefinition()
    {
        var configuration = ServiceProvider.GetRequiredService<IConfiguration>();
        var network = (NetworkEnum)Enum.Parse(typeof(NetworkEnum), configuration.GetValue<string>("Network"));

        return NetworkDefinition.Get(network);
    }
}
