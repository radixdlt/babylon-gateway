using FluentAssertions;
using RadixDlt.NetworkGateway.DataAggregator.Services;
using System.Collections.Generic;
using Xunit;

namespace RadixDlt.NetworkGateway.UnitTests.DataAggregator.Services;

public class PendingTransactionHashPairTests
{
    [Fact]
    public void X1()
    {
        var a = new PendingTransactionHashPair(new byte[] { 1, 2, 3 }, new byte[] { 4, 5, 6 });
        var b = new PendingTransactionHashPair(new byte[] { 1, 2, 3 }, new byte[] { 4, 5, 6 });

        a.GetHashCode().Should().Be(b.GetHashCode());
        a.Equals(b).Should().BeTrue();
    }

    [Fact]
    public void X2()
    {
        var a = new PendingTransactionHashPair(new byte[] { 1, 2, 3 }, new byte[] { 4, 5, 6 });
        var b = new PendingTransactionHashPair(new byte[] { 1, 2, 3 }, new byte[] { 4, 5, 1 });

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void X3()
    {
        var hs = new HashSet<PendingTransactionHashPair>
        {
            new(new byte[] { 1, 2, 3 }, new byte[] { 4, 5, 6 }),
            new(new byte[] { 1, 2, 3 }, new byte[] { 4, 5, 6 }),
        };

        hs.Count.Should().Be(1);
    }

    [Fact]
    public void X4()
    {
        var hs = new HashSet<PendingTransactionHashPair>
        {
            new(new byte[] { 1, 2, 3 }, new byte[] { 4, 5, 6 }),
            new(new byte[] { 1, 2, 1 }, new byte[] { 4, 5, 6 }),
        };

        hs.Count.Should().Be(2);
    }
}
