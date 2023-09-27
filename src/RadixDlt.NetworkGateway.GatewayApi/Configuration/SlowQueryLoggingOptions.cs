using FluentValidation;
using RadixDlt.NetworkGateway.Abstractions.Configuration;
using System;

namespace RadixDlt.NetworkGateway.GatewayApi.Configuration;

public sealed class SlowQueryLoggingOptions
{
    public TimeSpan SlowQueryThreshold { get; set; } = TimeSpan.FromMilliseconds(250);
}

internal class SlowQueryLoggingValidator : AbstractOptionsValidator<SlowQueryLoggingOptions>
{
    public SlowQueryLoggingValidator()
    {
        RuleFor(x => x.SlowQueryThreshold).GreaterThan(TimeSpan.Zero);
    }
}
