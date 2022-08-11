using FluentValidation;
using FluentValidation.Validators;
using System;
using System.Linq;

namespace RadixDlt.NetworkGateway.GatewayApi.Validators;

public class HexValidator<T> : PropertyValidator<T, string?>
{
    public override string Name => "HexValidator";

    private readonly int? _expectedLength;

    public HexValidator(int? expectedLength)
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

        if (!value.All(c => c is >= '0' and <= '9' or >= 'a' and <= 'f') || value.Length % 2 == 1)
        {
            context.AddFailure("'{PropertyName}' must be lowercase HEX-encoded value.");

            return false;
        }

        if (_expectedLength.HasValue)
        {
            var actual = Convert.FromHexString(value);

            if (actual.Length != _expectedLength)
            {
                context.MessageFormatter
                    .AppendArgument("ExpectedBytes", _expectedLength)
                    .AppendArgument("ActualBytes", actual.Length);

                context.AddFailure("'{PropertyName}' must be lowercase HEX-encoded representation of {ExpectedBytes} bytes. You entered {ActualBytes} bytes.");

                return false;
            }
        }

        return true;
    }
}
