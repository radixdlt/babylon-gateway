using FluentAssertions;
using FluentAssertions.Primitives;
using System;
using System.Diagnostics.CodeAnalysis;

namespace RadixDlt.NetworkGateway.UnitTests;

internal static class FluentAssertionsExtensions
{
    /// <summary>
    /// Asserts that the current object has been initialized.
    /// </summary>
    /// <param name="actualValue">
    /// Asserted object.
    /// </param>
    /// <param name="because">
    /// A formatted phrase as is supported by <see cref="string.Format(string,object[])" /> explaining why the assertion
    /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
    /// </param>
    /// <param name="becauseArgs">
    /// Zero or more objects to format using the placeholders in <paramref name="because" />.
    /// </param>
    /// <seealso href="https://github.com/fluentassertions/fluentassertions/issues/1115#issuecomment-925869230"/>
    public static AndConstraint<ObjectAssertions> ShouldNotBeNull([NotNull] this object? actualValue, string because = "", params object[] becauseArgs)
    {
        var result = actualValue.Should().NotBeNull(because, becauseArgs);

        if (actualValue == null)
        {
            throw new ArgumentNullException(nameof(actualValue)); // Will never be thrown, needed only to trick the compiler
        }

        return result;
    }
}
