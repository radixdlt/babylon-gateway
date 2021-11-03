namespace DataAggregator.Exceptions;

/// <summary>
/// An Exception thrown when the configuration provided to the service is incorrect.
/// </summary>
public class InvalidConfigurationException : Exception
{
    public InvalidConfigurationException(string message)
        : base(message)
    {
    }
}
