using DataAggregator.GlobalWorkers;
using Xunit;

namespace Tests.MiniUnitTests.DataAggregator.GlobalWorkers;

public class ExponentialBackoffDelayBetweenLoopsStrategyTests
{
    [Theory]
    [InlineData(1000, 2, 2, 2, 1, 1000)]
    [InlineData(1000, 2, 2, 2, 2, 1000)]
    [InlineData(1000, 2, 2, 2, 3, 2000)]
    [InlineData(1000, 2, 2, 2, 4, 4000)]
    [InlineData(1000, 0, 2, 4, 4, 16000)]
    [InlineData(2000, 0, 4, 4, 4, 512000)]
    [InlineData(2000, 0, 2, 2, 4, 8000)]
    [InlineData(2000, 0, 0, 0, 1, 2000)]
    [InlineData(2000, 0, 0, 0, 2, 2000)]
    [InlineData(2000, 0, 0, 0, 3, 2000)]
    [InlineData(2000, 0, 0, 0, 4, 2000)]
    public void ExponentialBackoff_IsCalculatedCorrectly(
        long baseDelayAfterErrorMs,
        int gracePeriod,
        uint rate,
        uint maxExponent,
        uint numErrors,
        long expectedMs)
    {
        var exponentialDelay = new ExponentialBackoffDelayBetweenLoopsStrategy(
            TimeSpan.Zero,
            TimeSpan.FromMilliseconds(baseDelayAfterErrorMs),
            gracePeriod,
            rate,
            maxExponent);

        Assert.Equal(
            TimeSpan.FromMilliseconds(expectedMs),
            exponentialDelay.DelayAfterError(TimeSpan.Zero, numErrors));
    }
}
