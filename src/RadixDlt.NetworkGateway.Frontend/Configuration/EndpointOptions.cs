using FluentValidation;
using Microsoft.Extensions.Configuration;
using RadixDlt.NetworkGateway.Configuration;

namespace RadixDlt.NetworkGateway.Frontend.Configuration;

public class EndpointOptions
{
    [ConfigurationKeyName("GatewayOpenApiSchemaVersion")]
    public string GatewayOpenApiSchemaVersion { get; set; } = "UNKNOWN";

    [ConfigurationKeyName("GatewayApiVersion")]
    public string GatewayApiVersion { get; set; } = "UNKNOWN";

    [ConfigurationKeyName("MaxPageSize")]
    public int MaxPageSize { get; set; } = 30;
}

internal class EndpointOptionsValidator : AbstractOptionsValidator<EndpointOptions>
{
    public EndpointOptionsValidator()
    {
        RuleFor(x => x.GatewayOpenApiSchemaVersion).NotNull();
        RuleFor(x => x.GatewayApiVersion).NotNull();
        RuleFor(x => x.MaxPageSize).GreaterThan(0);
    }
}
