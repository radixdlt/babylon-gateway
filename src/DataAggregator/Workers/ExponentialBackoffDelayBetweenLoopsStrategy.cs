namespace DataAggregator.Workers;

public class ExponentialBackoffDelayBetweenLoopsStrategy : IDelayBetweenLoopsStrategy
{
    private readonly TimeSpan _delayBetweenLoopTriggersIfSuccessful;
    private readonly TimeSpan _baseDelayAfterError;
    private readonly int _consecutiveErrorsAllowedBeforeExponentialBackoff;
    private readonly float _delayAfterErrorExponentialRate;
    private readonly TimeSpan _maxDelayAfterError;

    public ExponentialBackoffDelayBetweenLoopsStrategy(
        TimeSpan delayBetweenLoopTriggersIfSuccessful,
        TimeSpan baseDelayAfterError,
        int consecutiveErrorsAllowedBeforeExponentialBackoff,
        float delayAfterErrorExponentialRate,
        TimeSpan maxDelayAfterError
    )
    {
        if (baseDelayAfterError > maxDelayAfterError)
        {
            throw new ArgumentException("baseDelayAfterError can't be greater than maxDelayAfterError");
        }

        _delayBetweenLoopTriggersIfSuccessful = delayBetweenLoopTriggersIfSuccessful;
        _baseDelayAfterError = baseDelayAfterError;
        _consecutiveErrorsAllowedBeforeExponentialBackoff = consecutiveErrorsAllowedBeforeExponentialBackoff;
        _delayAfterErrorExponentialRate = delayAfterErrorExponentialRate;
        _maxDelayAfterError = maxDelayAfterError;
    }

    public TimeSpan DelayAfterSuccess(TimeSpan elapsedSinceLoopBeginning)
    {
        var delayRemaining = _delayBetweenLoopTriggersIfSuccessful - elapsedSinceLoopBeginning;
        return delayRemaining < TimeSpan.Zero ? TimeSpan.Zero : delayRemaining;
    }

    public TimeSpan DelayAfterError(TimeSpan elapsedSinceLoopBeginning, uint numConsecutiveErrors)
    {
        var totalDelay = numConsecutiveErrors <= _consecutiveErrorsAllowedBeforeExponentialBackoff
            ? _baseDelayAfterError
            : ExponentialDelayAfterError(numConsecutiveErrors);

        var delayRemaining = totalDelay - elapsedSinceLoopBeginning;
        return delayRemaining < TimeSpan.Zero ? TimeSpan.Zero : delayRemaining;
    }

    private TimeSpan ExponentialDelayAfterError(uint numConsecutiveErrors)
    {
        var numConsecutiveErrorsOverAllowed =
            numConsecutiveErrors - _consecutiveErrorsAllowedBeforeExponentialBackoff;
        var exponentialFactor = Math.Pow(
            _delayAfterErrorExponentialRate,
            numConsecutiveErrorsOverAllowed);
        var calculatedDelay = _baseDelayAfterError * exponentialFactor;
        return calculatedDelay > _maxDelayAfterError ? _maxDelayAfterError : calculatedDelay;
    }
}
