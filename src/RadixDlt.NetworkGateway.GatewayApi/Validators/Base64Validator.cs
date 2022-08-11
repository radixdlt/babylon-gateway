using FluentValidation;
using FluentValidation.Validators;
using System;

namespace RadixDlt.NetworkGateway.GatewayApi.Validators;

public class Base64Validator<T> : PropertyValidator<T, string?>
{
    public override string Name => "Base64Validator";

    private readonly int? _expectedLength;

    public Base64Validator(int? expectedLength)
    {
        _expectedLength = expectedLength;
    }

    public override bool IsValid(ValidationContext<T> context, string? value)
    {
        if (value == null)
        {
            return true;
        }

        context.MessageFormatter.AppendPropertyName(context.PropertyName);

        var buffer = new Span<byte>(new byte[value.Length]);

        if (!Convert.TryFromBase64String(value, buffer, out var bytesWritten))
        {
            context.AddFailure("'{PropertyName}' must be Base64-encoded value.");

            return false;
        }

        if (_expectedLength.HasValue && bytesWritten != _expectedLength.Value)
        {
            context.MessageFormatter
                .AppendArgument("ExpectedBytes", _expectedLength)
                .AppendArgument("ActualBytes", bytesWritten);

            context.AddFailure("'{PropertyName}' must be Base64-encoded representation of {ExpectedBytes} bytes. You entered {ActualBytes} bytes.");

            return false;
        }

        return true;
    }
}
