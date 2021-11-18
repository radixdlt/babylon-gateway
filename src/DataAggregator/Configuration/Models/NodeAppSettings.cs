using DataAggregator.Exceptions;

namespace DataAggregator.Configuration.Models;

public record NodeAppSettings
{
    /// <summary>
    /// A unique name identifying this node - used as the node's id.
    /// </summary>
    [ConfigurationKeyName("Name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Domain Name or IP Address.
    /// </summary>
    [ConfigurationKeyName("Address")]
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Relative weighting of the node.
    /// </summary>
    [ConfigurationKeyName("TrustWeighting")]
    public decimal TrustWeighting { get; set; } = 1;

    /// <summary>
    /// If false, the node should not be used for indexing.
    /// </summary>
    [ConfigurationKeyName("EnabledForIndexing")]
    public bool EnabledForIndexing { get; set; } = false;

    public void AssertValid()
    {
        if (string.IsNullOrEmpty(Name))
        {
            throw new InvalidConfigurationException("A node's name cannot be empty");
        }

        if (string.IsNullOrEmpty(Address))
        {
            throw new InvalidConfigurationException("A node's address cannot be empty");
        }
    }
}
