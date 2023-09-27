using FluentValidation;
using Microsoft.Extensions.Configuration;
using RadixDlt.NetworkGateway.Abstractions.Configuration;

namespace RadixDlt.NetworkGateway.DataAggregator.Configuration;

public sealed class NodeWorkersOptions
{
    [ConfigurationKeyName("ErrorStartupBlockTimeSeconds")]
    public int ErrorStartupBlockTimeSeconds { get; set; } = 20;

    [ConfigurationKeyName("GraceSecondsBeforeMarkingStalled")]
    public int GraceSecondsBeforeMarkingStalled { get; set; } = 10;
}

internal class NodeWorkersOptionsValidator : AbstractOptionsValidator<NodeWorkersOptions>
{
    public NodeWorkersOptionsValidator()
    {
        RuleFor(x => x.ErrorStartupBlockTimeSeconds).GreaterThan(0);
        RuleFor(x => x.GraceSecondsBeforeMarkingStalled).GreaterThan(0);
    }
}
