using FluentValidation;
using System;

namespace RadixDlt.NetworkGateway.UnitTests.GatewayApi.Validators;

public class TestValidator : AbstractValidator<TestSubject>
{
    public TestValidator(params Action<TestValidator>[] actions)
    {
        foreach (var action in actions)
        {
            action(this);
        }
    }
}

public class TestSubject
{
    public string? StringProperty { get; set; }
}
