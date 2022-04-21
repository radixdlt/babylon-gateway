using DataAggregator.Workers;
using Xunit;

namespace Tests.MiniUnitTests.DataAggregator.Workers;

public class ExponentialBackoffDelayBetweenLoopsStrategyTests
{
    [Theory]
    [InlineData(1000, 2, 2, 4000, 1, 1000)]
    [InlineData(1000, 2, 2, 4000, 2, 1000)]
    [InlineData(1000, 2, 2, 4000, 3, 2000)]
    [InlineData(1000, 2, 2, 4000, 4, 4000)]
    [InlineData(1000, 0, 2, 9500, 4, 9500)]
    [InlineData(2000, 0, 4, 513000, 4, 512000)]
    [InlineData(2000, 0, 2, 10000, 4, 10000)]
    [InlineData(2000, 0, 1, 10000, 1, 2000)]
    [InlineData(2000, 0, 1, 2000, 2, 2000)]
    [InlineData(2000, 0, 1, 2000, 3, 2000)]
    [InlineData(2000, 0, 1, 2000, 4, 2000)]
    public void ExponentialBackoff_IsCalculatedCorrectly(
        long baseDelayAfterErrorMs,
        int consecutiveErrorsAllowedBeforeExponentialBackoff,
        uint rate,
        uint maxDelayAfterErrorMs,
        uint numErrors,
        long expectedMs)
    {
        var exponentialDelay = new ExponentialBackoffDelayBetweenLoopsStrategy(
            TimeSpan.Zero,
            TimeSpan.FromMilliseconds(baseDelayAfterErrorMs),
            consecutiveErrorsAllowedBeforeExponentialBackoff,
            rate,
            TimeSpan.FromMilliseconds(maxDelayAfterErrorMs));

        Assert.Equal(
            TimeSpan.FromMilliseconds(expectedMs),
            exponentialDelay.DelayAfterError(TimeSpan.Zero, numErrors));
    }
}
