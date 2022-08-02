using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;

namespace RadixDlt.NetworkGateway.Configuration;

public class ApiOptions : IValidatableObject
{
    [Required]
    [ConfigurationKeyName("NetworkName")]
    public string NetworkName { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        yield break;
    }
}
