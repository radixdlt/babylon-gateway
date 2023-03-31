/* Copyright 2021 Radix Publishing Ltd incorporated in Jersey (Channel Islands).
 *
 * Licensed under the Radix License, Version 1.0 (the "License"); you may not use this
 * file except in compliance with the License. You may obtain a copy of the License at:
 *
 * radixfoundation.org/licenses/LICENSE-v1
 *
 * The Licensor hereby grants permission for the Canonical version of the Work to be
 * published, distributed and used under or by reference to the Licensor’s trademark
 * Radix ® and use of any unregistered trade names, logos or get-up.
 *
 * The Licensor provides the Work (and each Contributor provides its Contributions) on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied,
 * including, without limitation, any warranties or conditions of TITLE, NON-INFRINGEMENT,
 * MERCHANTABILITY, or FITNESS FOR A PARTICULAR PURPOSE.
 *
 * Whilst the Work is capable of being deployed, used and adopted (instantiated) to create
 * a distributed ledger it is your responsibility to test and validate the code, together
 * with all logic and performance of that code under all foreseeable scenarios.
 *
 * The Licensor does not make or purport to make and hereby excludes liability for all
 * and any representation, warranty or undertaking in any form whatsoever, whether express
 * or implied, to any entity or person, including any representation, warranty or
 * undertaking, as to the functionality security use, value or other characteristics of
 * any distributed ledger nor in respect the functioning or value of any tokens which may
 * be created stored or transferred using the Work. The Licensor does not warrant that the
 * Work or any use of the Work complies with any law or regulation in any territory where
 * it may be implemented or used or that it will be appropriate for any specific purpose.
 *
 * Neither the licensor nor any current or former employees, officers, directors, partners,
 * trustees, representatives, agents, advisors, contractors, or volunteers of the Licensor
 * shall be liable for any direct or indirect, special, incidental, consequential or other
 * losses of any kind, in tort, contract or otherwise (including but not limited to loss
 * of revenue, income or profits, or loss of use or data, or loss of reputation, or loss
 * of any economic or other opportunity of whatsoever nature or howsoever arising), arising
 * out of or in connection with (without limitation of any use, misuse, of any ledger system
 * or use made or its functionality or any performance or operation of any code or protocol
 * caused by bugs or programming or logic errors or otherwise);
 *
 * A. any offer, purchase, holding, use, sale, exchange or transmission of any
 * cryptographic keys, tokens or assets created, exchanged, stored or arising from any
 * interaction with the Work;
 *
 * B. any failure in a transmission or loss of any token or assets keys or other digital
 * artefacts due to errors in transmission;
 *
 * C. bugs, hacks, logic errors or faults in the Work or any communication;
 *
 * D. system software or apparatus including but not limited to losses caused by errors
 * in holding or transmitting tokens by any third-party;
 *
 * E. breaches or failure of security including hacker attacks, loss or disclosure of
 * password, loss of private key, unauthorised use or misuse of such passwords or keys;
 *
 * F. any losses including loss of anticipated savings or other benefits resulting from
 * use of the Work or any changes to the Work (however implemented).
 *
 * You are solely responsible for; testing, validating and evaluation of all operation
 * logic, functionality, security and appropriateness of using the Work for any commercial
 * or non-commercial purpose and for any reproduction or redistribution by You of the
 * Work. You assume all risks associated with Your use of the Work and the exercise of
 * permissions under this License.
 */

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
