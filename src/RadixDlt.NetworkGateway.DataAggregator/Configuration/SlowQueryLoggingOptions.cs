using FluentValidation;
using RadixDlt.NetworkGateway.Abstractions.Configuration;
using System;

namespace RadixDlt.NetworkGateway.DataAggregator.Configuration;

public sealed class SlowQueryLoggingOptions
{
    public TimeSpan SlowQueriesThreshold { get; set; } = TimeSpan.FromMilliseconds(250);
}

internal class SlowQueryLoggingOptionsValidator : AbstractOptionsValidator<SlowQueryLoggingOptions>
{
    public SlowQueryLoggingOptionsValidator()
    {
        RuleFor(x => x.SlowQueriesThreshold).GreaterThan(TimeSpan.Zero);
    }
}
