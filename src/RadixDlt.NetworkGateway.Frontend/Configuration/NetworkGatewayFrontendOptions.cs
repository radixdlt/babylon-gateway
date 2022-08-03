using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;

namespace RadixDlt.NetworkGateway.Frontend.Configuration;

public class NetworkGatewayFrontendOptions : IValidatableObject
{
    [Required]
    [ConfigurationKeyName("NetworkName")]
    public string NetworkName { get; set; } = null!;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(NetworkName))
        {
            yield return new ValidationResult("Cannot be empty.", new[] { nameof(NetworkName) });
        }
    }
}
