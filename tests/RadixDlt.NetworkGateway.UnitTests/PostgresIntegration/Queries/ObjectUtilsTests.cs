using FluentAssertions;
using RadixDlt.NetworkGateway.PostgresIntegration.Queries;
using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace RadixDlt.NetworkGateway.UnitTests.PostgresIntegration.Queries;

public class ObjectUtilsTests
{
    private class TestObject
    {
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "Follows the pattern used by the query result classes.")]
        public string? _stringField;
    }

    [Fact]
    public void SetOnceShouldSucceedWhenNotSet()
    {
        var obj = new TestObject();

        ObjectUtils.SetOnce(ref obj._stringField, "value");
    }

    [Fact]
    public void SetOnceShouldThrowWhenSet()
    {
        var obj = new TestObject();

        ObjectUtils.SetOnce(ref obj._stringField, "value");

        var action = () =>
        {
            ObjectUtils.SetOnce(ref obj._stringField, "value");
        };

        action.Should().Throw<ArgumentException>();
    }
}
