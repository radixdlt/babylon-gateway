using FluentValidation;
using Microsoft.Extensions.Configuration;
using RadixDlt.NetworkGateway.Configuration;

namespace RadixDlt.NetworkGateway.DataAggregator.Configuration;

public class MonitoringOptions
{
    [ConfigurationKeyName("StartupGracePeriodSeconds")]
    public int StartupGracePeriodSeconds { get; set; } = 10;

    [ConfigurationKeyName("UnhealthyCommitmentGapSeconds")]
    public int UnhealthyCommitmentGapSeconds { get; set; } = 20;
}

internal class MonitoringOptionsValidator : AbstractOptionsValidator<MonitoringOptions>
{
    public MonitoringOptionsValidator()
    {
        RuleFor(x => x.StartupGracePeriodSeconds).GreaterThan(0);
        RuleFor(x => x.UnhealthyCommitmentGapSeconds).GreaterThan(0);
    }
}
