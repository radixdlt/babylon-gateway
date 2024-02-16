using FluentAssertions;
using RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension;
using System.Collections.Generic;
using Xunit;

namespace RadixDlt.NetworkGateway.UnitTests.PostgresIntegration.LedgerExtension;

public class ChangeTrackerTests
{
    [Fact]
    public void PreservesInsertionOrder()
    {
        var expected = new List<KeyValuePair<string, int>>
        {
            new("b", 1),
            new("a", 2),
            new("c", 3),
        };

        var ct = new ChangeTracker<string, int>();

        foreach (var kvp in expected)
        {
            ct.GetOrAdd(kvp.Key, _ => kvp.Value);
        }

        ct.AsEnumerable().Should().Equal(expected);
    }

    [Fact]
    public void ReturnsExistingElement()
    {
        var ct = new ChangeTracker<string, int>();
        var a = ct.GetOrAdd("a", _ => 1);
        var b = ct.GetOrAdd("a", _ => 2);

        a.Should().Be(1);
        b.Should().Be(1);
    }
}
