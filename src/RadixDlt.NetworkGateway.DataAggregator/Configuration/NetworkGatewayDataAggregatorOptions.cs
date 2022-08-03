using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;

namespace RadixDlt.NetworkGateway.DataAggregator.Configuration;

public class NetworkGatewayDataAggregatorOptions : IValidatableObject
{
    public class CoreApiNode : IValidatableObject
    {
        [ConfigurationKeyName("Disabled")]
        public bool Disabled { get; }

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
        [ConfigurationKeyName("TrustWeighting")]
        public decimal TrustWeighting { get; set; } = 1;

        /// <summary>
        /// Relative weighting of the node.
        /// </summary>
        [ConfigurationKeyName("RequestWeighting")]
        public decimal RequestWeighting { get; set; } = 1;

        [ConfigurationKeyName("DisabledForTransactionIndexing")]
        public bool DisabledForTransactionIndexing { get; set; } = false;

        [ConfigurationKeyName("DisabledForTopOfTransactionReadingIfNotFullySynced")]
        public bool DisabledForTopOfTransactionReadingIfNotFullySynced { get; set; } = false;

        [ConfigurationKeyName("DisabledForMempool")]
        public bool DisabledForMempool { get; set; } = false;

        [ConfigurationKeyName("DisabledForMempoolUnknownTransactionFetching")]
        public bool DisabledForMempoolUnknownTransactionFetching { get; set; } = false;

        [ConfigurationKeyName("DisabledForConstruction")]
        public bool DisabledForConstruction { get; set; } = false;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Disabled)
            {
                yield break;
            }

            if (string.IsNullOrWhiteSpace(Name))
            {
                yield return new ValidationResult("Bla Bla required", new[] { nameof(Name) });
            }

            if (string.IsNullOrEmpty(CoreApiAddress))
            {
                yield return new ValidationResult("Bla Bla required", new[] { nameof(CoreApiAddress) });
            }
        }
    }

    [Required]
    [ConfigurationKeyName("NetworkName")]
    public string NetworkName { get; set; }

    [Required]
    [ConfigurationKeyName("CoreApiNodes")]
    public ICollection<CoreApiNode> CoreApiNodes { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // TODO force CoreApiNodes validation, see https://medium.com/@hina10531/dataannotation-recursive-validation-for-collection-items-47350368c327

        yield break;
    }
}


