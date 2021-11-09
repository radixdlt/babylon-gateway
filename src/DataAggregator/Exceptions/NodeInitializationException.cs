namespace DataAggregator.Exceptions;

/// <summary>
/// An Exception thrown when NodeWorker initialization throws a known exception.
/// </summary>
public class NodeInitializationException : Exception
{
    public NodeInitializationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public NodeInitializationException(string message)
        : base(message)
    {
    }
}
