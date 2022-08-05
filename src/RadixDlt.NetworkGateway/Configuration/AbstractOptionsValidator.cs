using FluentValidation;
using Microsoft.Extensions.Options;

namespace RadixDlt.NetworkGateway.Configuration;

public abstract class AbstractOptionsValidator<T> : AbstractValidator<T>, IValidateOptions<T>
    where T : class
{
    public virtual ValidateOptionsResult Validate(string name, T options)
    {
        var validateResult = Validate(options);

        return validateResult.IsValid ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(validateResult.Errors.Select(x => x.ErrorMessage));
    }
}
