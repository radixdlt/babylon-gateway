using FluentAssertions;
using RadixDlt.NetworkGateway.Abstractions.CoreCommunications;
using Xunit;
using CoreModel = RadixDlt.CoreApiSdk.Model;

namespace RadixDlt.NetworkGateway.UnitTests.Abstractions.CoreCommunications;

public class ResponseOrErrorTests
{
    [Fact]
    public void DefaultCtorSpecs()
    {
        var roe = new ResponseOrError<CoreModel.TransactionSubmitResponse, CoreModel.TransactionSubmitErrorResponse>();

        roe.Succeeded.Should().BeFalse();
        roe.Failed.Should().BeFalse();
        roe.SuccessResponse.Should().BeNull();
        roe.FailureResponse.Should().BeNull();
    }

    [Fact]
    public void OkSpecs()
    {
        var success = new CoreModel.TransactionSubmitResponse();
        var roe = ResponseOrError<CoreModel.TransactionSubmitResponse, CoreModel.TransactionSubmitErrorResponse>.Ok(success);

        roe.Succeeded.Should().BeTrue();
        roe.Failed.Should().BeFalse();
        roe.SuccessResponse.Should().BeSameAs(success);
        roe.FailureResponse.Should().BeNull();
    }

    [Fact]
    public void FailSpecs()
    {
        var error = new CoreModel.TransactionSubmitErrorResponse(message: "Something went wrong");
        var roe = ResponseOrError<CoreModel.TransactionSubmitResponse, CoreModel.TransactionSubmitErrorResponse>.Fail(error);

        roe.Succeeded.Should().BeFalse();
        roe.Failed.Should().BeTrue();
        roe.SuccessResponse.Should().BeNull();
        roe.FailureResponse.Should().BeSameAs(error);
    }
}
