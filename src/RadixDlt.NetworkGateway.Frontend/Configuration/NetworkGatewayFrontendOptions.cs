using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;

namespace RadixDlt.NetworkGateway.Frontend.Configuration;

public class NetworkGatewayFrontendOptions : IValidatableObject
{
    [Required]
    [ConfigurationKeyName("NetworkName")]
    public string NetworkName { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        yield break;
    }
}
