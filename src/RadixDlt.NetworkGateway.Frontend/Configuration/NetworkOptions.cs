using FluentValidation;
using Microsoft.Extensions.Configuration;
using RadixDlt.NetworkGateway.Configuration;

namespace RadixDlt.NetworkGateway.Frontend.Configuration;

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

    [ConfigurationKeyName("MaxAllowedStateVersionLagToBeConsideredSynced")]
    public long MaxAllowedStateVersionLagToBeConsideredSynced { get; set; } = 100;

    [ConfigurationKeyName("IgnoreNonSyncedNodes")]
    public bool IgnoreNonSyncedNodes { get; set; } = true;
}

public record CoreApiNode
{
    /// <summary>
    /// Whether the node's core API should be used to read from (defaults to true).
    /// </summary>
    [ConfigurationKeyName("Enabled")]
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// A unique name identifying this node - used as the node's id.
    /// </summary>
    [ConfigurationKeyName("Name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Address of the node's Core API.
    /// </summary>
    [ConfigurationKeyName("CoreApiAddress")]
    public string CoreApiAddress { get; set; } = string.Empty;

    /// <summary>
    /// AuthorizationHeader - if set, can allow for basic auth.
    /// </summary>
    [ConfigurationKeyName("CoreApiAuthorizationHeader")]
    public string? CoreApiAuthorizationHeader { get; set; } = null;

    /// <summary>
    /// Relative weighting of the node.
    /// </summary>
    [ConfigurationKeyName("RequestWeighting")]
    public decimal RequestWeighting { get; set; } = 1;
}

internal class NetworkOptionsValidator : AbstractOptionsValidator<NetworkOptions>
{
    public NetworkOptionsValidator()
    {
        RuleFor(x => x.NetworkName).NotNull();
        RuleFor(x => x.CoreApiNodes).NotNull();
        RuleForEach(x => x.CoreApiNodes).SetValidator(new CoreApiNodeOptionsValidator());
        RuleFor(x => x.MaxAllowedStateVersionLagToBeConsideredSynced).GreaterThan(0);
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
