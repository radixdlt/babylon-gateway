using FluentValidation.TestHelper;
using RadixDlt.NetworkGateway.GatewayApi.Validators;
using Xunit;

namespace RadixDlt.NetworkGateway.UnitTests.GatewayApi.Validators;

public class Base64ValidatorTests
{
    [Fact]
    public void WhenGiven_NullValue_Succeeds()
    {
        var validator = new TestValidator(v => v.RuleFor(x => x.StringProperty).Base64());
        var result = validator.TestValidate(new TestSubject { StringProperty = null });

        result.ShouldNotHaveValidationErrorFor(x => x.StringProperty);
    }

    [Theory]
    [InlineData("")]
    [InlineData("YQ==")]
    [InlineData("aGVsbG8=")]
    [InlineData("R0lGODlhAQABAAAAACw=")]
    [InlineData("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII=")]
    public void WhenGiven_ValidValue_Succeeds(string hex)
    {
        var validator = new TestValidator(v => v.RuleFor(x => x.StringProperty).Base64());
        var result = validator.TestValidate(new TestSubject { StringProperty = hex });

        result.ShouldNotHaveValidationErrorFor(x => x.StringProperty);
    }

    [Theory]
    [InlineData("", 0)]
    [InlineData("YQ==", 1)]
    [InlineData("aGVsbG8=", 5)]
    [InlineData("R0lGODlhAQABAAAAACw=", 14)]
    [InlineData("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII=", 68)]
    public void WhenGiven_HexValueOfExpectedLength_Succeeds(string hex, int expectedLength)
    {
        var validator = new TestValidator(v => v.RuleFor(x => x.StringProperty).Base64(expectedLength));
        var result = validator.TestValidate(new TestSubject { StringProperty = hex });

        result.ShouldNotHaveValidationErrorFor(x => x.StringProperty);
    }

    [Theory]
    [InlineData("YQ")]
    [InlineData("YQ=")]
    [InlineData("Yś==")]
    public void WhenGiven_InvalidBase64Value_Fails(string hex)
    {
        var validator = new TestValidator(v => v.RuleFor(x => x.StringProperty).Base64());
        var result = validator.TestValidate(new TestSubject { StringProperty = hex });

        result.ShouldHaveValidationErrorFor(x => x.StringProperty);
    }

    [Fact]
    public void WhenGiven_ValidBase64ValueOfInvalidLength_Fails()
    {
        var validator = new TestValidator(v => v.RuleFor(x => x.StringProperty).Base64(15));
        var result = validator.TestValidate(new TestSubject { StringProperty = "R0lGODlhAQABAAAAACw=" });

        result.ShouldHaveValidationErrorFor(x => x.StringProperty);
    }
}
