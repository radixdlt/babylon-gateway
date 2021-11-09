using DataAggregator.Configuration.Models;
using DataAggregator.Exceptions;

namespace DataAggregator.Configuration;

public interface IAggregatorConfiguration
{
    List<NodeAppSettings> GetNodes();

    int GetNetworkId();
}

public class AggregatorConfiguration : IAggregatorConfiguration
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AggregatorConfiguration> _logger;

    public AggregatorConfiguration(IConfiguration configuration, ILogger<AggregatorConfiguration> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public List<NodeAppSettings> GetNodes()
    {
        var nodesSection = _configuration.GetSection("Nodes");
        if (!nodesSection.Exists())
        {
            throw new InvalidConfigurationException("appsettings.json requires a Nodes section");
        }

        var nodesList = new List<NodeAppSettings>();
        nodesSection.Bind(nodesList);

        if (!nodesList.Any())
        {
            _logger.LogWarning("appsettings.json Nodes section is empty");
        }

        nodesList.ForEach(n => n.AssertValid());
        return nodesList;
    }

    public int GetNetworkId()
    {
        var networkId = _configuration.GetValue<int?>("NetworkId", null);
        if (networkId == null)
        {
            throw new InvalidConfigurationException("appsettings.json requires an integer NetworkId");
        }

        return (int)networkId;
    }
}
