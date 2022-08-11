using FluentValidation.TestHelper;
using RadixDlt.NetworkGateway.GatewayApi.Validators;
using Xunit;

namespace RadixDlt.NetworkGateway.UnitTests.GatewayApi.Validators;

public class HexValidatorTests
{
    [Fact]
    public void WhenGiven_NullValue_Succeeds()
    {
        var validator = new TestValidator(v => v.RuleFor(x => x.StringProperty).Hex());
        var result = validator.TestValidate(new TestSubject { StringProperty = null });

        result.ShouldNotHaveValidationErrorFor(x => x.StringProperty);
    }

    [Theory]
    [InlineData("")]
    [InlineData("ab")]
    [InlineData("0123456789abcdef")]
    [InlineData("0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef")]
    public void WhenGiven_ValidValue_Succeeds(string hex)
    {
        var validator = new TestValidator(v => v.RuleFor(x => x.StringProperty).Hex());
        var result = validator.TestValidate(new TestSubject { StringProperty = hex });

        result.ShouldNotHaveValidationErrorFor(x => x.StringProperty);
    }

    [Theory]
    [InlineData("", 0)]
    [InlineData("ab", 1)]
    [InlineData("0123456789abcdef", 8)]
    [InlineData("0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef", 40)]
    public void WhenGiven_ValidValueWithExpectedLength_Succeeds(string hex, int expectedLength)
    {
        var validator = new TestValidator(v => v.RuleFor(x => x.StringProperty).Hex(expectedLength));
        var result = validator.TestValidate(new TestSubject { StringProperty = hex });

        result.ShouldNotHaveValidationErrorFor(x => x.StringProperty);
    }

    [Theory]
    [InlineData("a")]
    [InlineData("abc")]
    [InlineData("AB")]
    [InlineData("base64characters")]
    [InlineData("abcdefP")]
    public void WhenGiven_InvalidValue_Fails(string hex)
    {
        var validator = new TestValidator(v => v.RuleFor(x => x.StringProperty).Hex());
        var result = validator.TestValidate(new TestSubject { StringProperty = hex });

        result.ShouldHaveValidationErrorFor(x => x.StringProperty);
    }

    [Fact]
    public void WhenGiven_ValidValueOfInvalidLength_Fails()
    {
        var validator = new TestValidator(v => v.RuleFor(x => x.StringProperty).Hex(8));
        var result = validator.TestValidate(new TestSubject { StringProperty = "0123456789abcd" });

        result.ShouldHaveValidationErrorFor(x => x.StringProperty);
    }
}
