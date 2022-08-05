using FluentValidation;
using Microsoft.Extensions.Configuration;
using RadixDlt.NetworkGateway.Configuration;

namespace RadixDlt.NetworkGateway.DataAggregator.Configuration;

public class NetworkOptions
{
    [ConfigurationKeyName("NetworkName")]
    public string NetworkName { get; set; } = null!;

    [ConfigurationKeyName("CoreApiNodes")]
    public ICollection<CoreApiNode> CoreApiNodes { get; set; } = new List<CoreApiNode>();

    [ConfigurationKeyName("DisableCoreApiHttpsCertificateChecks")]
    public bool DisableCoreApiHttpsCertificateChecks { get; set; }

    [ConfigurationKeyName("CoreApiHttpProxyAddress")]
    public string? CoreApiHttpProxyAddress { get; set; }
}

public record CoreApiNode
{
    /// <summary>
    /// If false, the node should not be used.
    /// </summary>
    [ConfigurationKeyName("Enabled")]
    public bool Enabled { get; set; }

    /// <summary>
    /// A unique name identifying this node - used as the node's id.
    /// </summary>
    [ConfigurationKeyName("Name")]
    public string Name { get; set; } = null!;

    /// <summary>
    /// Address of the node's Core API.
    /// </summary>
    [ConfigurationKeyName("CoreApiAddress")]
    public string CoreApiAddress { get; set; } = null!;

    /// <summary>
    /// AuthorizationHeader - if set, can allow for basic auth.
    /// </summary>
    [ConfigurationKeyName("CoreApiAuthorizationHeader")]
    public string? CoreApiAuthorizationHeader { get; set; }

    /// <summary>
    /// Relative weighting of the node.
    /// </summary>
    [ConfigurationKeyName("TrustWeighting")]
    public decimal TrustWeighting { get; set; } = 1;

    /// <summary>
    /// Relative weighting of the node.
    /// </summary>
    [ConfigurationKeyName("RequestWeighting")]
    public decimal RequestWeighting { get; set; } = 1;

    [ConfigurationKeyName("DisabledForTransactionIndexing")]
    public bool DisabledForTransactionIndexing { get; set; }

    [ConfigurationKeyName("DisabledForTopOfTransactionReadingIfNotFullySynced")]
    public bool DisabledForTopOfTransactionReadingIfNotFullySynced { get; set; }

    [ConfigurationKeyName("DisabledForMempool")]
    public bool DisabledForMempool { get; set; }

    [ConfigurationKeyName("DisabledForMempoolUnknownTransactionFetching")]
    public bool DisabledForMempoolUnknownTransactionFetching { get; set; }

    [ConfigurationKeyName("DisabledForConstruction")]
    public bool DisabledForConstruction { get; set; }
}

internal class NetworkOptionsValidator : AbstractOptionsValidator<NetworkOptions>
{
    public NetworkOptionsValidator()
    {
        RuleFor(x => x.NetworkName).NotNull();
        RuleFor(x => x.CoreApiNodes).NotNull();
        RuleForEach(x => x.CoreApiNodes).SetValidator(new CoreApiNodeOptionsValidator());
    }
}

internal class CoreApiNodeOptionsValidator : AbstractOptionsValidator<CoreApiNode>
{
    public CoreApiNodeOptionsValidator()
    {
        When(x => x.Enabled, () =>
        {
            RuleFor(x => x.Name).NotNull();
            RuleFor(x => x.CoreApiAddress).NotNull();
        });
    }
}
