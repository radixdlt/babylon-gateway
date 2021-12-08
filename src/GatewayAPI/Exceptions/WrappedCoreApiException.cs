using Core = RadixCoreApi.Generated.Model;
using CoreClient = RadixCoreApi.Generated.Client;

namespace GatewayAPI.Exceptions;

/// <summary>
/// A marker exception to be caught / handled in other code.
/// We use this rather than the ApiException itself so that we have a typesafe CoreError we can use in other places.
/// </summary>
/// <typeparam name="T">The type of the core error.</typeparam>
public class WrappedCoreApiException<T> : WrappedCoreApiException
    where T : Core.CoreError
{
    public override T Error { get; }

    public WrappedCoreApiException(CoreClient.ApiException apiException, T error)
        : base($"Core API reported a {typeof(T).Name}", apiException)
    {
        Error = error;
    }
}

public abstract class WrappedCoreApiException : Exception
{
    public abstract Core.CoreError Error { get; }

    public CoreClient.ApiException ApiException { get; }

    public WrappedCoreApiException(string message, CoreClient.ApiException apiException)
        : base(message, apiException)
    {
        ApiException = apiException;
    }

    public static WrappedCoreApiException<T> Of<T>(CoreClient.ApiException apiException, T error)
        where T : Core.CoreError
    {
        return new WrappedCoreApiException<T>(apiException, error);
    }
}
