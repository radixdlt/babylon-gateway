using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;

namespace RadixDlt.NetworkGateway.Configuration;

public class DataAggregatorOptions : IValidatableObject
{
    public class CoreApiNode : IValidatableObject
    {
        [ConfigurationKeyName("Disabled")]
        public bool Disabled { get; }

        [ConfigurationKeyName("Name")]
        public string Name { get; set; }

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
        yield break;
    }
}


