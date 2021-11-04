using DataAggregator.Configuration.Models;
using DataAggregator.NodeScopedServices;

namespace DataAggregator.DependencyInjection;

public interface INodeWorkersRunnerFactory
{
    NodeWorkersRunner CreateWorkersForNode(NodeAppSettings initialNodeAppSettings);
}

/// <summary>
/// Creates a DI Scope for the Node, and initialises the NodeConfigProvider with the initial settings.
/// It then creates the NodeWorkersRunner, which takes on responsibility for disposing the scope when it's done.
/// </summary>
public class NodeWorkersRunnerFactory : INodeWorkersRunnerFactory
{
    private readonly ILogger<NodeWorkersRunner> _logger;
    private readonly IServiceProvider _services;

    // ReSharper disable once ContextualLoggerProblem - Done on purpose as we're passing it to the NodeWorkersRunner
    public NodeWorkersRunnerFactory(ILogger<NodeWorkersRunner> logger, IServiceProvider services)
    {
        _logger = logger;
        _services = services;
    }

    public NodeWorkersRunner CreateWorkersForNode(NodeAppSettings initialNodeAppSettings)
    {
        var nodeScope = _services.CreateScope();
        nodeScope.ServiceProvider.GetRequiredService<INodeConfigProvider>().NodeAppSettings = initialNodeAppSettings;
        var logScope = _logger.BeginScope($"[NODE: {initialNodeAppSettings.Name}]");
        return new NodeWorkersRunner(_logger, nodeScope, logScope);
    }
}
