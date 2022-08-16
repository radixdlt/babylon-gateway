using FluentValidation.TestHelper;
using RadixDlt.NetworkGateway.GatewayApi.Validators;
using RadixDlt.NetworkGateway.GatewayApiSdk.Model;
using RadixDlt.NetworkGateway.UnitTests.Common;
using System;
using Xunit;

namespace RadixDlt.NetworkGateway.UnitTests.GatewayApi.Validators;

public class PartialLedgerStateIdentifierValidatorTests
{
    private readonly PartialLedgerStateIdentifierValidator _validator;

    public PartialLedgerStateIdentifierValidatorTests()
    {
        _validator = new PartialLedgerStateIdentifierValidator(new FakeClock(DateTimeOffset.Parse("2020-06-06T12:00:00Z")));
    }

    [Theory]
    [InlineData(1, null, null, null)]
    [InlineData(10000, null, null, null)]
    [InlineData(null, "2020-06-06T11:59:59Z", null, null)]
    [InlineData(null, "2000-06-06T12:00:00Z", null, null)]
    [InlineData(null, null, 1, null)]
    [InlineData(null, null, 10000, null)]
    [InlineData(null, null, 10000, 1)]
    [InlineData(null, null, 10000, 10000)]
    public void WhenGiven_ValidPartialLedgerStateIdentifier_ShouldNotHaveAnyValidationErrors(long? stateVersion, string? timestamp, long? epoch, long? round)
    {
        var model = new PartialLedgerStateIdentifier(stateVersion, timestamp != null ? DateTimeOffset.Parse(timestamp) : null, epoch, round);

        _validator.TestValidate(model).ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null, null, null, null)]
    [InlineData(1, "2020-06-06T11:59:59Z", 1, 1)]
    [InlineData(1, "2020-06-06T11:59:59Z", null, null)]
    [InlineData(1, "2020-06-06T11:59:59Z", 1, null)]
    [InlineData(1, null, 1, null)]
    [InlineData(1, null, null, 1)]
    [InlineData(null, "2020-06-06T11:59:59Z", 1, 1)]
    [InlineData(null, "2020-06-06T11:59:59Z", 1, null)]
    [InlineData(null, null, null, 1)]
    [InlineData(0, null, null, null)]
    [InlineData(-1, null, null, null)]
    [InlineData(null, "2020-06-06T12:00:01Z", null, null)]
    [InlineData(null, "2020-06-06T13:00:00Z", null, null)]
    [InlineData(null, null, 0, null)]
    [InlineData(null, null, -1, null)]
    [InlineData(null, null, 23, 0)]
    [InlineData(null, null, 23, -1)]
    public void WhenGiven_InvalidPartialLedgerStateIdentifier_ShouldHaveAnyValidationError(long? stateVersion, string? timestamp, long? epoch, long? round)
    {
        var model = new PartialLedgerStateIdentifier(stateVersion, timestamp != null ? DateTimeOffset.Parse(timestamp) : null, epoch, round);

        _validator.TestValidate(model).ShouldHaveAnyValidationError();
    }
}
