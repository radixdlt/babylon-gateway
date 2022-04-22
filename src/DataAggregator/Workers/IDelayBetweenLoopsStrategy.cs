namespace DataAggregator.Workers;

public interface IDelayBetweenLoopsStrategy
{
    public static IDelayBetweenLoopsStrategy ConstantDelayStrategy(
        TimeSpan delayBetweenLoopTriggersIfSuccessful, TimeSpan delayBetweenLoopTriggersIfError)
    {
        // Reusing exponential backoff strategy with a rate of 1
        return ExponentialDelayStrategy(
            delayBetweenLoopTriggersIfSuccessful: delayBetweenLoopTriggersIfSuccessful,
            baseDelayAfterError: delayBetweenLoopTriggersIfError,
            consecutiveErrorsAllowedBeforeExponentialBackoff: 0,
            delayAfterErrorExponentialRate: 1,
            maxDelayAfterError: delayBetweenLoopTriggersIfError);
    }

    public static IDelayBetweenLoopsStrategy ExponentialDelayStrategy(
        TimeSpan delayBetweenLoopTriggersIfSuccessful,
        TimeSpan baseDelayAfterError,
        int consecutiveErrorsAllowedBeforeExponentialBackoff,
        float delayAfterErrorExponentialRate,
        TimeSpan maxDelayAfterError)
    {
        return new ExponentialBackoffDelayBetweenLoopsStrategy(
            delayBetweenLoopTriggersIfSuccessful,
            baseDelayAfterError,
            consecutiveErrorsAllowedBeforeExponentialBackoff,
            delayAfterErrorExponentialRate,
            maxDelayAfterError);
    }

    TimeSpan DelayAfterSuccess(TimeSpan elapsedSinceLoopBeginning);

    TimeSpan DelayAfterError(TimeSpan elapsedSinceLoopBeginning, uint numConsecutiveErrors);
}
