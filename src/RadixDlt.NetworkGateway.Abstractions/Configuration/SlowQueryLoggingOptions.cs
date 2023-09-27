using FluentValidation;
using System;

namespace RadixDlt.NetworkGateway.Abstractions.Configuration;

public sealed class SlowQueryLoggingOptions
{
    public TimeSpan SlowQueryThreshold { get; set; } = TimeSpan.FromMilliseconds(250);
}

public class SlowQueryLoggingOptionsValidator : AbstractOptionsValidator<SlowQueryLoggingOptions>
{
    public SlowQueryLoggingOptionsValidator()
    {
        RuleFor(x => x.SlowQueryThreshold).GreaterThan(TimeSpan.Zero);
    }
}
