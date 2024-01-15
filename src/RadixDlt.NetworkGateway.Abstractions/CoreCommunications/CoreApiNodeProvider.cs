using RadixDlt.NetworkGateway.Abstractions.Configuration;
using RadixDlt.NetworkGateway.Abstractions.Exceptions;

namespace RadixDlt.NetworkGateway.Abstractions.CoreCommunications;

public interface ICoreApiNodeProvider
{
    public CoreApiNode CoreApiNode { get; }
}

public interface ICoreApiNodeConfigurator
{
    public CoreApiNode CoreApiNode { set; }
}

public class CoreApiNodeProvider : ICoreApiNodeProvider, ICoreApiNodeConfigurator
{
    private CoreApiNode? _node;

    public CoreApiNode CoreApiNode
    {
        get => _node ?? throw new InvalidNodeStateException("CoreApiNode in CoreApiNodeProvider should be set at init time");
        set => _node = value;
    }
}
