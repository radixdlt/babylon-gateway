using DataAggregator.Configuration.Models;

namespace DataAggregator.NodeScopedServices;

public interface INodeConfigProvider
{
    public NodeAppSettings NodeAppSettings { get; set; }
}

public class NodeConfigProvider : INodeConfigProvider
{
    private NodeAppSettings? _nodeAppSettings;

    public NodeAppSettings NodeAppSettings
    {
        get => _nodeAppSettings ?? throw new Exception("NodeAppSettings in NodeConfigProvider should be set at init time");
        set => _nodeAppSettings = value;
    }
}
