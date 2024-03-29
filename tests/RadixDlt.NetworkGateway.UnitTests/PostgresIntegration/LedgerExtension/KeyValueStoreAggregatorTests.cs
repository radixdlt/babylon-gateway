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
using Moq;
using RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using System.Collections.Generic;
using System.Threading;
using Xunit;

namespace RadixDlt.NetworkGateway.UnitTests.PostgresIntegration.LedgerExtension;

public class KeyValueStoreAggregatorTests
{
    [Fact]
    public void NoDataInDatabase_MultipleKeyValueStoresCreated()
    {
        // Arrange.
        var preTestHistoryId = 200;
        var preTestAggregateId = 100;
        var sequences = new SequencesHolder
        {
            KeyValueStoreEntryHistorySequence = preTestHistoryId,
            KeyValueStoreAggregateHistorySequence = preTestAggregateId,
        };
        var context = new ProcessorContext(sequences, new Mock<IReadHelper>().Object, new Mock<IWriteHelper>().Object, CancellationToken.None);

        var mostRecentEntries = new Dictionary<KeyValueStoreEntryDbLookup, KeyValueStoreEntryHistory>();
        var mostRecentAggregates = new Dictionary<long, KeyValueStoreAggregateHistory>();

        var changes = new List<KeyValueStoreChange>
        {
            new(KeyValueStoreEntityId: 10, StateVersion: 100, KeyValueStoreExtensions.GenerateKeyEntry(1)),
            new(KeyValueStoreEntityId: 10, StateVersion: 100, KeyValueStoreExtensions.GenerateKeyEntry(2)),
            new(KeyValueStoreEntityId: 10, StateVersion: 100, KeyValueStoreExtensions.GenerateKeyEntry(3)),
            new(KeyValueStoreEntityId: 11, StateVersion: 100, KeyValueStoreExtensions.GenerateKeyEntry(1)),
            new(KeyValueStoreEntityId: 11, StateVersion: 100, KeyValueStoreExtensions.GenerateKeyEntry(10)),
            new(KeyValueStoreEntityId: 12, StateVersion: 110, KeyValueStoreExtensions.GenerateKeyEntry(10)),
        };

        var expectedEntries = new List<KeyValueStoreEntryHistory>()
        {
            KeyValueStoreExtensions.CreateDatabaseHistoryEntry(id: 200, keyValueStoreEntityId: 10, fromStateVersion: 100, KeyValueStoreExtensions.GenerateKeyEntry(1)),
            KeyValueStoreExtensions.CreateDatabaseHistoryEntry(id: 201, keyValueStoreEntityId: 10, fromStateVersion: 100, KeyValueStoreExtensions.GenerateKeyEntry(2)),
            KeyValueStoreExtensions.CreateDatabaseHistoryEntry(id: 202, keyValueStoreEntityId: 10, fromStateVersion: 100, KeyValueStoreExtensions.GenerateKeyEntry(3)),
            KeyValueStoreExtensions.CreateDatabaseHistoryEntry(id: 203, keyValueStoreEntityId: 11, fromStateVersion: 100, KeyValueStoreExtensions.GenerateKeyEntry(1)),
            KeyValueStoreExtensions.CreateDatabaseHistoryEntry(id: 204, keyValueStoreEntityId: 11, fromStateVersion: 100, KeyValueStoreExtensions.GenerateKeyEntry(10)),
            KeyValueStoreExtensions.CreateDatabaseHistoryEntry(id: 205, keyValueStoreEntityId: 12, fromStateVersion: 110, KeyValueStoreExtensions.GenerateKeyEntry(10)),
        };

        var expectedAggregate = new List<KeyValueStoreAggregateHistory>
        {
            new() { Id = 100, FromStateVersion = 100, KeyValueStoreEntityId = 10, KeyValueStoreEntryIds = new List<long> { 200, 201, 202 } },
            new() { Id = 101, FromStateVersion = 100, KeyValueStoreEntityId = 11, KeyValueStoreEntryIds = new List<long> { 203, 204 } },
            new() { Id = 102, FromStateVersion = 110, KeyValueStoreEntityId = 12, KeyValueStoreEntryIds = new List<long> { 205 } },
        };

        var changeTracker = KeyValueStoreExtensions.PrepareChanges(changes);

        // Act.
        var (entriesToAdd, aggregatesToAdd) = KeyValueStoreAggregator.Aggregate(context, changeTracker, mostRecentEntries, mostRecentAggregates);

        // Assert.
        entriesToAdd.Should().BeEquivalentTo(expectedEntries);
        aggregatesToAdd.Should().BeEquivalentTo(expectedAggregate);
    }

    [Fact]
    public void KeyValueStoreWithExistingData_OneKeyIsDeleted()
    {
        // Arrange.
        var preTestHistoryId = 200;
        var preTestAggregateId = 100;
        var sequences = new SequencesHolder
        {
            KeyValueStoreEntryHistorySequence = preTestHistoryId,
            KeyValueStoreAggregateHistorySequence = preTestAggregateId,
        };
        var context = new ProcessorContext(sequences, new Mock<IReadHelper>().Object, new Mock<IWriteHelper>().Object, CancellationToken.None);

        var mostRecentEntries = new Dictionary<KeyValueStoreEntryDbLookup, KeyValueStoreEntryHistory>
        {
            {
                new KeyValueStoreEntryDbLookup(10, KeyValueStoreExtensions.GenerateKeyEntry(1).Key),
                KeyValueStoreExtensions.CreateDatabaseHistoryEntry(id: 100, keyValueStoreEntityId: 10, fromStateVersion: 100, KeyValueStoreExtensions.GenerateDeletedKeyEntry(1))
            },
            {
                new KeyValueStoreEntryDbLookup(10, KeyValueStoreExtensions.GenerateKeyEntry(2).Key),
                KeyValueStoreExtensions.CreateDatabaseHistoryEntry(id: 101, keyValueStoreEntityId: 10, fromStateVersion: 100, KeyValueStoreExtensions.GenerateDeletedKeyEntry(2))
            },
        };

        var mostRecentAggregates = new Dictionary<long, KeyValueStoreAggregateHistory>
        {
            { 10, new KeyValueStoreAggregateHistory { Id = 50, FromStateVersion = 100, KeyValueStoreEntityId = 10, KeyValueStoreEntryIds = new List<long> { 100, 101 } } },
        };

        var changes = new List<KeyValueStoreChange>
        {
            new(KeyValueStoreEntityId: 10, StateVersion: 200, KeyValueStoreExtensions.GenerateDeletedKeyEntry(1)),
        };

        var expectedEntriesToAdd = new List<KeyValueStoreEntryHistory>
        {
            KeyValueStoreExtensions.CreateDatabaseHistoryEntry(id: 200, keyValueStoreEntityId: 10, fromStateVersion: 200, KeyValueStoreExtensions.GenerateDeletedKeyEntry(1), isDeleted: true),
        };

        var expectedAggregatesToAdd = new List<KeyValueStoreAggregateHistory>
        {
            new() { Id = 100, FromStateVersion = 200, KeyValueStoreEntityId = 10, KeyValueStoreEntryIds = new List<long> { 101 } },
        };

        var changeTracker = KeyValueStoreExtensions.PrepareChanges(changes);

        // Act.
        var (entriesToAdd, aggregatesToAdd) = KeyValueStoreAggregator.Aggregate(context, changeTracker, mostRecentEntries, mostRecentAggregates);

        // Assert.
        entriesToAdd.Should().BeEquivalentTo(expectedEntriesToAdd);
        aggregatesToAdd.Should().BeEquivalentTo(expectedAggregatesToAdd);
    }

    [Fact]
    public void KeyValueStoreWithExistingData_OneKeyIsDeleted_AndRecreatedTwice()
    {
        // Arrange.
        var preTestHistoryId = 200;
        var preTestAggregateId = 100;
        var sequences = new SequencesHolder
        {
            KeyValueStoreEntryHistorySequence = preTestHistoryId,
            KeyValueStoreAggregateHistorySequence = preTestAggregateId,
        };
        var context = new ProcessorContext(sequences, new Mock<IReadHelper>().Object, new Mock<IWriteHelper>().Object, CancellationToken.None);

        var mostRecentEntries = new Dictionary<KeyValueStoreEntryDbLookup, KeyValueStoreEntryHistory>
        {
            {
                new KeyValueStoreEntryDbLookup(10, KeyValueStoreExtensions.GenerateKeyEntry(1).Key),
                KeyValueStoreExtensions.CreateDatabaseHistoryEntry(id: 100, keyValueStoreEntityId: 10, fromStateVersion: 100, KeyValueStoreExtensions.GenerateDeletedKeyEntry(1))
            },
            {
                new KeyValueStoreEntryDbLookup(10, KeyValueStoreExtensions.GenerateKeyEntry(2).Key),
                KeyValueStoreExtensions.CreateDatabaseHistoryEntry(id: 101, keyValueStoreEntityId: 10, fromStateVersion: 100, KeyValueStoreExtensions.GenerateDeletedKeyEntry(2))
            },
        };

        var mostRecentAggregates = new Dictionary<long, KeyValueStoreAggregateHistory>
        {
            { 10, new KeyValueStoreAggregateHistory { Id = 50, FromStateVersion = 100, KeyValueStoreEntityId = 10, KeyValueStoreEntryIds = new List<long> { 100, 101 } } },
        };

        var changes = new List<KeyValueStoreChange>
        {
            new(KeyValueStoreEntityId: 10, StateVersion: 200, KeyValueStoreExtensions.GenerateDeletedKeyEntry(1)),
            new(KeyValueStoreEntityId: 10, StateVersion: 250, KeyValueStoreExtensions.GenerateKeyEntry(1, 11)),
            new(KeyValueStoreEntityId: 10, StateVersion: 300, KeyValueStoreExtensions.GenerateDeletedKeyEntry(1)),
            new(KeyValueStoreEntityId: 10, StateVersion: 350, KeyValueStoreExtensions.GenerateKeyEntry(1, 12)),
        };

        var expectedEntriesToAdd = new List<KeyValueStoreEntryHistory>
        {
            KeyValueStoreExtensions.CreateDatabaseHistoryEntry(id: 200, keyValueStoreEntityId: 10, fromStateVersion: 200, KeyValueStoreExtensions.GenerateDeletedKeyEntry(1), isDeleted: true),
            KeyValueStoreExtensions.CreateDatabaseHistoryEntry(id: 201, keyValueStoreEntityId: 10, fromStateVersion: 250, KeyValueStoreExtensions.GenerateKeyEntry(1, 11)),
            KeyValueStoreExtensions.CreateDatabaseHistoryEntry(id: 202, keyValueStoreEntityId: 10, fromStateVersion: 300, KeyValueStoreExtensions.GenerateDeletedKeyEntry(1), isDeleted: true),
            KeyValueStoreExtensions.CreateDatabaseHistoryEntry(id: 203, keyValueStoreEntityId: 10, fromStateVersion: 350, KeyValueStoreExtensions.GenerateKeyEntry(1, 12)),
        };

        var expectedAggregatesToAdd = new List<KeyValueStoreAggregateHistory>
        {
            new() { Id = 100, FromStateVersion = 200, KeyValueStoreEntityId = 10, KeyValueStoreEntryIds = new List<long> { 101 } },
            new() { Id = 101, FromStateVersion = 250, KeyValueStoreEntityId = 10, KeyValueStoreEntryIds = new List<long> { 101, 201 } },
            new() { Id = 102, FromStateVersion = 300, KeyValueStoreEntityId = 10, KeyValueStoreEntryIds = new List<long> { 101 } },
            new() { Id = 103, FromStateVersion = 350, KeyValueStoreEntityId = 10, KeyValueStoreEntryIds = new List<long> { 101, 203 } },
        };

        var changeTracker = KeyValueStoreExtensions.PrepareChanges(changes);

        // Act.
        var (entriesToAdd, aggregatesToAdd) = KeyValueStoreAggregator.Aggregate(context, changeTracker, mostRecentEntries, mostRecentAggregates);

        // Assert.
        entriesToAdd.Should().BeEquivalentTo(expectedEntriesToAdd);
        aggregatesToAdd.Should().BeEquivalentTo(expectedAggregatesToAdd);
    }

    [Fact]
    public void CombinedTest()
    {
        // Arrange.
        var preTestHistoryId = 200;
        var preTestAggregateId = 100;
        var sequences = new SequencesHolder
        {
            KeyValueStoreEntryHistorySequence = preTestHistoryId,
            KeyValueStoreAggregateHistorySequence = preTestAggregateId,
        };
        var context = new ProcessorContext(sequences, new Mock<IReadHelper>().Object, new Mock<IWriteHelper>().Object, CancellationToken.None);

        var mostRecentEntries = new Dictionary<KeyValueStoreEntryDbLookup, KeyValueStoreEntryHistory>
        {
            {
                new KeyValueStoreEntryDbLookup(10, KeyValueStoreExtensions.GenerateKeyEntry(1).Key),
                KeyValueStoreExtensions.CreateDatabaseHistoryEntry(id: 100, keyValueStoreEntityId: 10, fromStateVersion: 100, KeyValueStoreExtensions.GenerateDeletedKeyEntry(1))
            },
            {
                new KeyValueStoreEntryDbLookup(10, KeyValueStoreExtensions.GenerateKeyEntry(2).Key),
                KeyValueStoreExtensions.CreateDatabaseHistoryEntry(id: 101, keyValueStoreEntityId: 10, fromStateVersion: 100, KeyValueStoreExtensions.GenerateDeletedKeyEntry(2))
            },
        };

        var mostRecentAggregates = new Dictionary<long, KeyValueStoreAggregateHistory>
        {
            { 10, new KeyValueStoreAggregateHistory { Id = 50, FromStateVersion = 100, KeyValueStoreEntityId = 10, KeyValueStoreEntryIds = new List<long> { 100, 101 } } },
        };

        var changes = new List<KeyValueStoreChange>
        {
            // new entries.
            new(KeyValueStoreEntityId: 20, StateVersion: 100, KeyValueStoreExtensions.GenerateKeyEntry(1)),
            new(KeyValueStoreEntityId: 20, StateVersion: 100, KeyValueStoreExtensions.GenerateKeyEntry(2)),
            new(KeyValueStoreEntityId: 20, StateVersion: 100, KeyValueStoreExtensions.GenerateKeyEntry(3)),
            new(KeyValueStoreEntityId: 21, StateVersion: 100, KeyValueStoreExtensions.GenerateKeyEntry(1)),
            new(KeyValueStoreEntityId: 21, StateVersion: 100, KeyValueStoreExtensions.GenerateKeyEntry(10)),
            new(KeyValueStoreEntityId: 22, StateVersion: 110, KeyValueStoreExtensions.GenerateKeyEntry(10)),
            new(KeyValueStoreEntityId: 20, StateVersion: 200, KeyValueStoreExtensions.GenerateKeyEntry(6)),
            new(KeyValueStoreEntityId: 32, StateVersion: 200, KeyValueStoreExtensions.GenerateKeyEntry(367)),

            // deletes of existing entries.
            new(KeyValueStoreEntityId: 10, StateVersion: 200, KeyValueStoreExtensions.GenerateDeletedKeyEntry(1)),
            new(KeyValueStoreEntityId: 10, StateVersion: 250, KeyValueStoreExtensions.GenerateKeyEntry(1, 11)),
            new(KeyValueStoreEntityId: 10, StateVersion: 300, KeyValueStoreExtensions.GenerateDeletedKeyEntry(1)),
            new(KeyValueStoreEntityId: 10, StateVersion: 350, KeyValueStoreExtensions.GenerateKeyEntry(1, 12)),
            new(KeyValueStoreEntityId: 10, StateVersion: 400, KeyValueStoreExtensions.GenerateDeletedKeyEntry(1)),
            new(KeyValueStoreEntityId: 10, StateVersion: 400, KeyValueStoreExtensions.GenerateDeletedKeyEntry(2)),
            new(KeyValueStoreEntityId: 10, StateVersion: 401, KeyValueStoreExtensions.GenerateKeyEntry(1, 13)),
        };

        var expectedEntriesToAdd = new List<KeyValueStoreEntryHistory>
        {
            // new entries.
            KeyValueStoreExtensions.CreateDatabaseHistoryEntry(id: 200, keyValueStoreEntityId: 20, fromStateVersion: 100, KeyValueStoreExtensions.GenerateKeyEntry(1)),
            KeyValueStoreExtensions.CreateDatabaseHistoryEntry(id: 201, keyValueStoreEntityId: 20, fromStateVersion: 100, KeyValueStoreExtensions.GenerateKeyEntry(2)),
            KeyValueStoreExtensions.CreateDatabaseHistoryEntry(id: 202, keyValueStoreEntityId: 20, fromStateVersion: 100, KeyValueStoreExtensions.GenerateKeyEntry(3)),
            KeyValueStoreExtensions.CreateDatabaseHistoryEntry(id: 203, keyValueStoreEntityId: 21, fromStateVersion: 100, KeyValueStoreExtensions.GenerateKeyEntry(1)),
            KeyValueStoreExtensions.CreateDatabaseHistoryEntry(id: 204, keyValueStoreEntityId: 21, fromStateVersion: 100, KeyValueStoreExtensions.GenerateKeyEntry(10)),
            KeyValueStoreExtensions.CreateDatabaseHistoryEntry(id: 205, keyValueStoreEntityId: 22, fromStateVersion: 110, KeyValueStoreExtensions.GenerateKeyEntry(10)),
            KeyValueStoreExtensions.CreateDatabaseHistoryEntry(id: 206, keyValueStoreEntityId: 20, fromStateVersion: 200, KeyValueStoreExtensions.GenerateKeyEntry(6)),
            KeyValueStoreExtensions.CreateDatabaseHistoryEntry(id: 207, keyValueStoreEntityId: 32, fromStateVersion: 200, KeyValueStoreExtensions.GenerateKeyEntry(367)),

            // deletes of existing entries.
            KeyValueStoreExtensions.CreateDatabaseHistoryEntry(id: 208, keyValueStoreEntityId: 10, fromStateVersion: 200, KeyValueStoreExtensions.GenerateDeletedKeyEntry(1), isDeleted: true),
            KeyValueStoreExtensions.CreateDatabaseHistoryEntry(id: 209, keyValueStoreEntityId: 10, fromStateVersion: 250, KeyValueStoreExtensions.GenerateKeyEntry(1, 11)),
            KeyValueStoreExtensions.CreateDatabaseHistoryEntry(id: 210, keyValueStoreEntityId: 10, fromStateVersion: 300, KeyValueStoreExtensions.GenerateDeletedKeyEntry(1), isDeleted: true),
            KeyValueStoreExtensions.CreateDatabaseHistoryEntry(id: 211, keyValueStoreEntityId: 10, fromStateVersion: 350, KeyValueStoreExtensions.GenerateKeyEntry(1, 12)),
            KeyValueStoreExtensions.CreateDatabaseHistoryEntry(id: 212, keyValueStoreEntityId: 10, fromStateVersion: 400, KeyValueStoreExtensions.GenerateDeletedKeyEntry(1), isDeleted: true),
            KeyValueStoreExtensions.CreateDatabaseHistoryEntry(id: 213, keyValueStoreEntityId: 10, fromStateVersion: 400, KeyValueStoreExtensions.GenerateDeletedKeyEntry(2), isDeleted: true),
            KeyValueStoreExtensions.CreateDatabaseHistoryEntry(id: 214, keyValueStoreEntityId: 10, fromStateVersion: 401, KeyValueStoreExtensions.GenerateKeyEntry(1, 13)),
        };

        var expectedAggregatesToAdd = new List<KeyValueStoreAggregateHistory>
        {
            // new entries.
            new() { Id = 100, FromStateVersion = 100, KeyValueStoreEntityId = 20, KeyValueStoreEntryIds = new List<long> { 200, 201, 202 } },
            new() { Id = 101, FromStateVersion = 100, KeyValueStoreEntityId = 21, KeyValueStoreEntryIds = new List<long> { 203, 204 } },
            new() { Id = 102, FromStateVersion = 110, KeyValueStoreEntityId = 22, KeyValueStoreEntryIds = new List<long> { 205 } },
            new() { Id = 103, FromStateVersion = 200, KeyValueStoreEntityId = 20, KeyValueStoreEntryIds = new List<long> { 200, 201, 202, 206 } },
            new() { Id = 104, FromStateVersion = 200, KeyValueStoreEntityId = 32, KeyValueStoreEntryIds = new List<long> { 207 } },

            // deletes of existing entries.
            new() { Id = 105, FromStateVersion = 200, KeyValueStoreEntityId = 10, KeyValueStoreEntryIds = new List<long> { 101 } },
            new() { Id = 106, FromStateVersion = 250, KeyValueStoreEntityId = 10, KeyValueStoreEntryIds = new List<long> { 101, 209 } },
            new() { Id = 107, FromStateVersion = 300, KeyValueStoreEntityId = 10, KeyValueStoreEntryIds = new List<long> { 101 } },
            new() { Id = 108, FromStateVersion = 350, KeyValueStoreEntityId = 10, KeyValueStoreEntryIds = new List<long> { 101, 211 } },
            new() { Id = 109, FromStateVersion = 400, KeyValueStoreEntityId = 10, KeyValueStoreEntryIds = new List<long>() },
            new() { Id = 110, FromStateVersion = 401, KeyValueStoreEntityId = 10, KeyValueStoreEntryIds = new List<long> { 214 } },
        };

        var changeTracker = KeyValueStoreExtensions.PrepareChanges(changes);

        // Act.
        var (entriesToAdd, aggregatesToAdd) = KeyValueStoreAggregator.Aggregate(context, changeTracker, mostRecentEntries, mostRecentAggregates);

        // Assert.
        entriesToAdd.Should().BeEquivalentTo(expectedEntriesToAdd);
        aggregatesToAdd.Should().BeEquivalentTo(expectedAggregatesToAdd);
    }
}
