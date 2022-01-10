namespace Common.Exceptions;

public class AppFatalExceptionDetectedException : Exception
{
    public AppFatalExceptionDetectedException(Exception innerException)
        : base("An app fatal exception has been detected", innerException)
    {
    }
}
