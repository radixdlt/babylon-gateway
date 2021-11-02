using DataAggregator.Configuration.Models;
using DataAggregator.Exceptions;

namespace DataAggregator.Configuration;

public class AggregatorConfiguration
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
}
