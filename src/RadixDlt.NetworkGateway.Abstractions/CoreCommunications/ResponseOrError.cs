using System.Diagnostics.CodeAnalysis;

namespace RadixDlt.NetworkGateway.Abstractions.CoreCommunications;

public readonly struct ResponseOrError<TResponse, TError>
    where TResponse : class
    where TError : CoreApiSdk.Model.ErrorResponse
{
    public TResponse? SuccessResponse { get; }

    public TError? FailureResponse { get; }

    [MemberNotNullWhen(true, nameof(SuccessResponse))]
    [MemberNotNullWhen(false, nameof(FailureResponse))]
    public bool Succeeded => SuccessResponse != null;

    [MemberNotNullWhen(false, nameof(SuccessResponse))]
    [MemberNotNullWhen(true, nameof(FailureResponse))]
    public bool Failed => FailureResponse != null;

    private ResponseOrError(TResponse? successResponse, TError? failureResponse)
    {
        SuccessResponse = successResponse;
        FailureResponse = failureResponse;
    }

    public static ResponseOrError<TResponse, TError> Ok(TResponse result) => new(result, default);

    public static ResponseOrError<TResponse, TError> Fail(TError error) => new(default, error);
}
