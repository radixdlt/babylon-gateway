using FluentAssertions;
using RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension;
using System.Collections.Generic;
using Xunit;

namespace RadixDlt.NetworkGateway.UnitTests.PostgresIntegration.LedgerExtension;

public class ExtensionsTests
{
    [Fact]
    public void GetOrAdd_Specs()
    {
        var factoryCalls = 0;

        int Factory(int key)
        {
            factoryCalls++;

            return 3;
        }

        var dict = new Dictionary<int, int>();
        var a = dict.GetOrAdd(5, Factory);
        var b = dict.GetOrAdd(5, Factory);

        a.Should().Be(3);
        b.Should().Be(3);
        factoryCalls.Should().Be(1);
    }

    private record MyTuple(byte A, int B);

    private record MyTriple(byte A, int B, long C);

    [Fact]
    public void Unzip2_ShouldNotAllocateOnEmpty()
    {
        var lookupSet = new HashSet<MyTuple>();
        var res = lookupSet.Unzip(x => x.A, x => x.B, out var a, out var b);

        res.Should().BeFalse();
        a.Should().BeNull();
        b.Should().BeNull();
    }

    [Fact]
    public void Unzip2_Specs()
    {
        var lookupSet = new HashSet<MyTuple>
        {
            new MyTuple(1, 2),
            new MyTuple(3, 2),
            new MyTuple(1, 1),
        };

        var res = lookupSet.Unzip(x => x.A, x => x.B, out var a, out var b);

        res.Should().BeTrue();
        a.Should().Equal(new List<byte> { 1, 3, 1 });
        b.Should().Equal(new List<int> { 2, 2, 1 });
    }

    [Fact]
    public void Unzip3_ShouldNotAllocateOnEmpty()
    {
        var lookupSet = new HashSet<MyTriple>();
        var res = lookupSet.Unzip(x => x.A, x => x.B, x => x.C, out var a, out var b, out var c);

        res.Should().BeFalse();
        a.Should().BeNull();
        b.Should().BeNull();
        c.Should().BeNull();
    }

    [Fact]
    public void Unzip3_Specs()
    {
        var lookupSet = new HashSet<MyTriple>
        {
            new MyTriple(1, 2, 3),
            new MyTriple(3, 2, 1),
            new MyTriple(1, 1, 1),
        };

        var res = lookupSet.Unzip(x => x.A, x => x.B, x => x.C, out var a, out var b, out var c);

        res.Should().BeTrue();
        a.Should().Equal(new List<byte> { 1, 3, 1 });
        b.Should().Equal(new List<int> { 2, 2, 1 });
        c.Should().Equal(new List<long> { 3, 1, 1 });
    }
}
