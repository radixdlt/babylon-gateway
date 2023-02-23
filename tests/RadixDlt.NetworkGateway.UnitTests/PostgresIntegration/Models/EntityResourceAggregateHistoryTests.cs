using FluentAssertions;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using System.Collections.Generic;
using Xunit;

namespace RadixDlt.NetworkGateway.UnitTests.PostgresIntegration.Models;

public class EntityResourceAggregateHistoryTests
{
    private readonly EntityResourceAggregateHistory _aggregateWithAmbiguous;
    private readonly EntityResourceAggregateHistory _aggregateWithoutAmbiguous;
    private readonly EntityResourceAggregateHistory _derivedWithAmbiguous;
    private readonly EntityResourceAggregateHistory _derivedWithoutAmbiguous;

    public EntityResourceAggregateHistoryTests()
    {
        _aggregateWithAmbiguous = new EntityResourceAggregateHistory
        {
            Id = 1,
            FromStateVersion = 1000,
            FungibleResourceEntityIds = new List<long>(new long[] { 1, 2, 3 }),
            FungibleResourceSignificantUpdateStateVersions = new List<long>(new long[] { 1000, 1000, 500 }),
            NonFungibleResourceEntityIds = new List<long>(new long[] { 4, 5, 6 }),
            NonFungibleResourceSignificantUpdateStateVersions = new List<long>(new long[] { 1000, 1000, 500 }),
        };

        _aggregateWithoutAmbiguous = new EntityResourceAggregateHistory
        {
            Id = 2,
            FromStateVersion = 1000,
            FungibleResourceEntityIds = new List<long>(new long[] { 1, 2, 3 }),
            FungibleResourceSignificantUpdateStateVersions = new List<long>(new long[] { 1000, 750, 500 }),
            NonFungibleResourceEntityIds = new List<long>(new long[] { 4, 5, 6 }),
            NonFungibleResourceSignificantUpdateStateVersions = new List<long>(new long[] { 1000, 750, 500 }),
        };

        _derivedWithAmbiguous = EntityResourceAggregateHistory.CopyOf(3, _aggregateWithAmbiguous, 2000);
        _derivedWithoutAmbiguous = EntityResourceAggregateHistory.CopyOf(4, _aggregateWithoutAmbiguous, 2000);
    }

    [Fact]
    public void ShouldBePersisted_GivenNewlyCreatedInstance_ReturnsTrue()
    {
        _aggregateWithAmbiguous.ShouldBePersisted().Should().BeTrue();
        _aggregateWithoutAmbiguous.ShouldBePersisted().Should().BeTrue();
    }

    [Fact]
    public void ShouldBePersisted_GivenCopiedAndUnalteredInstance_ReturnsFalse()
    {
        _derivedWithAmbiguous.ShouldBePersisted().Should().BeFalse();
        _derivedWithoutAmbiguous.ShouldBePersisted().Should().BeFalse();
    }

    [Fact]
    public void CopyOf_PreservesResourcesAndTheirUpdates()
    {
        _derivedWithAmbiguous.FungibleResourceEntityIds.Should().Equal(_aggregateWithAmbiguous.FungibleResourceEntityIds);
        _derivedWithAmbiguous.FungibleResourceSignificantUpdateStateVersions.Should().Equal(_aggregateWithAmbiguous.FungibleResourceSignificantUpdateStateVersions);
        _derivedWithAmbiguous.NonFungibleResourceEntityIds.Should().Equal(_aggregateWithAmbiguous.NonFungibleResourceEntityIds);
        _derivedWithAmbiguous.NonFungibleResourceSignificantUpdateStateVersions.Should().Equal(_aggregateWithAmbiguous.NonFungibleResourceSignificantUpdateStateVersions);
        _derivedWithAmbiguous.ShouldBePersisted().Should().BeFalse();
    }

    [Fact]
    public void TryUpsertNewFungibleResourceSpecs()
    {
        _derivedWithAmbiguous.TryUpsertFungible(99, 1001).Should().BeTrue();
        _derivedWithAmbiguous.FungibleResourceEntityIds.Should().Equal(new List<long>(new long[] { 99, 1, 2, 3 }));
        _derivedWithAmbiguous.FungibleResourceSignificantUpdateStateVersions.Should().Equal(new List<long>(new long[] { 1001, 1000, 1000, 500 }));
        _derivedWithAmbiguous.ShouldBePersisted().Should().BeTrue();
    }

    [Fact]
    public void TryUpsertExistingOutdatedFungibleResourceSpecs()
    {
        _derivedWithAmbiguous.TryUpsertFungible(3, 1001).Should().BeTrue();
        _derivedWithAmbiguous.FungibleResourceEntityIds.Should().Equal(new List<long>(new long[] { 3, 1, 2 }));
        _derivedWithAmbiguous.FungibleResourceSignificantUpdateStateVersions.Should().Equal(new List<long>(new long[] { 1001, 1000, 1000 }));
        _derivedWithAmbiguous.ShouldBePersisted().Should().BeTrue();
    }

    [Fact]
    public void TryUpsertExistingMostRecentAmbiguousFungibleResourceSpecs()
    {
        _derivedWithAmbiguous.TryUpsertFungible(1, 1001).Should().BeTrue();
        _derivedWithAmbiguous.FungibleResourceEntityIds.Should().Equal(new List<long>(new long[] { 1, 2, 3 }));
        _derivedWithAmbiguous.FungibleResourceSignificantUpdateStateVersions.Should().Equal(new List<long>(new long[] { 1001, 1000, 500 }));
        _derivedWithAmbiguous.ShouldBePersisted().Should().BeFalse();
    }

    [Fact]
    public void TryUpsertExistingMostRecentUnambiguousFungibleResourceSpecs()
    {
        _derivedWithoutAmbiguous.TryUpsertFungible(1, 1001).Should().BeFalse();
        _derivedWithoutAmbiguous.FungibleResourceEntityIds.Should().Equal(new List<long>(new long[] { 1, 2, 3 }));
        _derivedWithoutAmbiguous.FungibleResourceSignificantUpdateStateVersions.Should().Equal(new List<long>(new long[] { 1000, 750, 500 }));
        _derivedWithoutAmbiguous.ShouldBePersisted().Should().BeFalse();
    }

    [Fact]
    public void TryUpsertConsecutiveFungibleResourceSpecs()
    {
        _derivedWithAmbiguous.TryUpsertFungible(1, 1001).Should().BeTrue();
        _derivedWithAmbiguous.FungibleResourceEntityIds.Should().Equal(new List<long>(new long[] { 1, 2, 3 }));
        _derivedWithAmbiguous.FungibleResourceSignificantUpdateStateVersions.Should().Equal(new List<long>(new long[] { 1001, 1000, 500 }));
        _derivedWithAmbiguous.ShouldBePersisted().Should().BeFalse();

        _derivedWithAmbiguous.TryUpsertFungible(1, 1002).Should().BeFalse();
        _derivedWithAmbiguous.FungibleResourceEntityIds.Should().Equal(new List<long>(new long[] { 1, 2, 3 }));
        _derivedWithAmbiguous.FungibleResourceSignificantUpdateStateVersions.Should().Equal(new List<long>(new long[] { 1001, 1000, 500 }));
        _derivedWithAmbiguous.ShouldBePersisted().Should().BeFalse();
    }

    [Fact]
    public void TryUpsertNewNonFungibleResourceSpecs()
    {
        _derivedWithAmbiguous.TryUpsertNonFungible(99, 1001).Should().BeTrue();
        _derivedWithAmbiguous.NonFungibleResourceEntityIds.Should().Equal(new List<long>(new long[] { 99, 4, 5, 6 }));
        _derivedWithAmbiguous.NonFungibleResourceSignificantUpdateStateVersions.Should().Equal(new List<long>(new long[] { 1001, 1000, 1000, 500 }));
        _derivedWithAmbiguous.ShouldBePersisted().Should().BeTrue();
    }

    [Fact]
    public void TryUpsertExistingOutdatedNonFungibleResourceSpecs()
    {
        _derivedWithAmbiguous.TryUpsertNonFungible(6, 1001).Should().BeTrue();
        _derivedWithAmbiguous.NonFungibleResourceEntityIds.Should().Equal(new List<long>(new long[] { 6, 4, 5 }));
        _derivedWithAmbiguous.NonFungibleResourceSignificantUpdateStateVersions.Should().Equal(new List<long>(new long[] { 1001, 1000, 1000 }));
        _derivedWithAmbiguous.ShouldBePersisted().Should().BeTrue();
    }

    [Fact]
    public void TryUpsertExistingMostRecentAmbiguousNonFungibleResourceSpecs()
    {
        _derivedWithAmbiguous.TryUpsertNonFungible(4, 1001).Should().BeTrue();
        _derivedWithAmbiguous.NonFungibleResourceEntityIds.Should().Equal(new List<long>(new long[] { 4, 5, 6 }));
        _derivedWithAmbiguous.NonFungibleResourceSignificantUpdateStateVersions.Should().Equal(new List<long>(new long[] { 1001, 1000, 500 }));
        _derivedWithAmbiguous.ShouldBePersisted().Should().BeFalse();
    }

    [Fact]
    public void TryUpsertExistingMostRecentUnambiguousNonFungibleResourceSpecs()
    {
        _derivedWithoutAmbiguous.TryUpsertNonFungible(4, 1001).Should().BeFalse();
        _derivedWithoutAmbiguous.NonFungibleResourceEntityIds.Should().Equal(new List<long>(new long[] { 4, 5, 6 }));
        _derivedWithoutAmbiguous.NonFungibleResourceSignificantUpdateStateVersions.Should().Equal(new List<long>(new long[] { 1000, 750, 500 }));
        _derivedWithoutAmbiguous.ShouldBePersisted().Should().BeFalse();
    }

    [Fact]
    public void TryUpsertConsecutiveNonFungibleResourceSpecs()
    {
        _derivedWithAmbiguous.TryUpsertNonFungible(4, 1001).Should().BeTrue();
        _derivedWithAmbiguous.NonFungibleResourceEntityIds.Should().Equal(new List<long>(new long[] { 4, 5, 6 }));
        _derivedWithAmbiguous.NonFungibleResourceSignificantUpdateStateVersions.Should().Equal(new List<long>(new long[] { 1001, 1000, 500 }));
        _derivedWithAmbiguous.ShouldBePersisted().Should().BeFalse();

        _derivedWithAmbiguous.TryUpsertNonFungible(4, 1002).Should().BeFalse();
        _derivedWithAmbiguous.NonFungibleResourceEntityIds.Should().Equal(new List<long>(new long[] { 4, 5, 6 }));
        _derivedWithAmbiguous.NonFungibleResourceSignificantUpdateStateVersions.Should().Equal(new List<long>(new long[] { 1001, 1000, 500 }));
        _derivedWithAmbiguous.ShouldBePersisted().Should().BeFalse();
    }
}
