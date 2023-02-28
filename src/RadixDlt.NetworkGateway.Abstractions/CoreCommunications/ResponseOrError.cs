using System.Diagnostics.CodeAnalysis;

namespace RadixDlt.NetworkGateway.Abstractions.CoreCommunications;

public readonly struct ResponseOrError<TResponse, TError>
    where TResponse : class
    where TError : CoreApiSdk.Model.ErrorResponse
{
    public TResponse? Success { get; }

    public TError? Failure { get; }

    [MemberNotNullWhen(true, nameof(Success))]
    [MemberNotNullWhen(false, nameof(Failure))]
    public bool Succeeded => Success != null;

    [MemberNotNullWhen(false, nameof(Success))]
    [MemberNotNullWhen(true, nameof(Failure))]
    public bool Failed => Failure != null;

    private ResponseOrError(TResponse? success, TError? failure)
    {
        Success = success;
        Failure = failure;
    }

    public static ResponseOrError<TResponse, TError> Ok(TResponse result) => new(result, default);

    public static ResponseOrError<TResponse, TError> Fail(TError error) => new(default, error);
}
