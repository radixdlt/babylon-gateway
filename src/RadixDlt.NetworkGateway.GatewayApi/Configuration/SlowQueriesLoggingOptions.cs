using FluentValidation;
using RadixDlt.NetworkGateway.Abstractions.Configuration;
using System;

namespace RadixDlt.NetworkGateway.GatewayApi.Configuration;

public sealed class SlowQueriesLoggingOptions
{
    public TimeSpan SlowQueriesThreshold { get; set; } = TimeSpan.FromMilliseconds(250);
}

internal class SlowQueriesLoggingOptionsValidator : AbstractOptionsValidator<SlowQueriesLoggingOptions>
{
    public SlowQueriesLoggingOptionsValidator()
    {
        RuleFor(x => x.SlowQueriesThreshold).GreaterThan(TimeSpan.Zero);
    }
}
