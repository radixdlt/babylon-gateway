using Microsoft.Extensions.Logging;

namespace Common.Utilities;

/// <summary>
/// Outputs occasionalLogLevel greedily at most once per timespan, otherwise outputs noisyLogLevel. Thread safe.
/// </summary>
public class LogLimiter
{
    private readonly TimeSpan _timespan;
    private readonly LogLevel _occasionalLogLevel;
    private readonly LogLevel _noisyLogLevel;
    private readonly object _lock = new();
    private DateTime? _notBefore;

    public LogLimiter(TimeSpan timespan, LogLevel occasionalLogLevel, LogLevel noisyLogLevel)
    {
        _timespan = timespan;
        _occasionalLogLevel = occasionalLogLevel;
        _noisyLogLevel = noisyLogLevel;
    }

    public LogLevel GetLogLevel()
    {
        lock (_lock)
        {
            var currTime = DateTime.Now;
            if (LogShouldBeMarkedAsNoisy(currTime))
            {
                return _noisyLogLevel;
            }

            _notBefore = currTime.Add(_timespan);
            return _occasionalLogLevel;
        }
    }

    private bool LogShouldBeMarkedAsNoisy(DateTime currTime)
    {
        // ReSharper disable once MergeSequentialChecks
        return _notBefore != null && currTime < _notBefore;
    }
}
