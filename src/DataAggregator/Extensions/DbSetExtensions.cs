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

using Common.Database;
using Common.Database.Models.Ledger;
using Common.Database.Models.Ledger.History;
using Common.Database.Models.Ledger.Substates;
using Common.Extensions;
using DataAggregator.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DataAggregator.Extensions;

public static class DbSetExtensions
{
    public static async Task UpSubstate<TSubstate>(
        this DbSet<TSubstate> substates,
        TransactionOpLocator transactionOpLocator,
        byte[] identifier,
        TSubstate newSubstate,
        LedgerOperationGroup upOperationGroup,
        int upOperationIndexInGroup,
        CancellationToken cancellationToken
    )
        where TSubstate : SubstateBase
    {
        // Could rely on the database to check this constraint at commit time, but this gives us a clearer error
        var existingSubstate = substates
                                   .Local
                                   .SingleOrDefault(s => s.SubstateIdentifier.BytesAreEqual(identifier))
                               ?? await substates
                                   .AsNoTracking()
                                   .SingleOrDefaultAsync(s => s.SubstateIdentifier == identifier, cancellationToken);
        if (existingSubstate != null)
        {
            throw new InvalidTransactionException(
                transactionOpLocator,
                $"{typeof(TSubstate).FullName} with identifier {identifier.ToHex()} can't be upped, as a substate with that identifier already already exists in the database"
            );
        }

        newSubstate.SubstateIdentifier = identifier;
        newSubstate.UpOperationGroup = upOperationGroup;
        newSubstate.UpOperationIndexInGroup = upOperationIndexInGroup;

        substates.Add(newSubstate);
    }

    public static async Task DownSubstate<TSubstate>(
        this DbSet<TSubstate> substates,
        TransactionOpLocator transactionOpLocator,
        byte[] identifier,
        Func<TSubstate> createNewSubstateIfVirtual,
        Func<TSubstate, bool> verifySubstateMatches,
        LedgerOperationGroup downOperationGroup,
        int downOperationIndexInGroup,
        CancellationToken cancellationToken
    )
        where TSubstate : SubstateBase
    {
        var substate = substates
                           .Local
                           .SingleOrDefault(s => s.SubstateIdentifier.BytesAreEqual(identifier))
                       ?? await substates
                           .SingleOrDefaultAsync(s => s.SubstateIdentifier == identifier, cancellationToken);
        if (substate == null)
        {
            if (!SubstateBase.IsVirtualIdentifier(identifier))
            {
                throw new InvalidTransactionException(
                    transactionOpLocator,
                    $"Non-virtual {typeof(TSubstate).Name} with identifier {identifier.ToHex()} could not be downed as it did not exist in the database"
                );
            }

            // Virtual substates can be downed without being upped
            var newSubstate = createNewSubstateIfVirtual();
            newSubstate.SubstateIdentifier = identifier;
            newSubstate.UpOperationGroup = downOperationGroup;
            newSubstate.UpOperationIndexInGroup = downOperationIndexInGroup;
            newSubstate.DownOperationGroup = downOperationGroup;
            newSubstate.DownOperationIndexInGroup = downOperationIndexInGroup;
            substates.Add(newSubstate);
            return;
        }

        if (substate.State == SubstateState.Down)
        {
            throw new InvalidTransactionException(
                transactionOpLocator,
                $"{typeof(TSubstate).Name} with identifier {identifier.ToHex()} could not be downed as it was already down"
            );
        }

        if (!verifySubstateMatches(substate))
        {
            throw new InvalidTransactionException(
                transactionOpLocator,
                $"{typeof(TSubstate).Name} with identifier {identifier.ToHex()} was downed, but the substate contents appear not to match at downing time"
            );
        }

        substate.DownOperationGroup = downOperationGroup;
        substate.DownOperationIndexInGroup = downOperationIndexInGroup;
    }

    /// <summary>
    /// Creates a new history entry and closes out the previous one (if it exists).
    ///
    /// Note that:
    /// * historySelector does not need to care about the StateVersion.
    /// * createNewHistory does not need to care about the StateVersion.
    /// </summary>
    /// <typeparam name="THistory">The history entity type.</typeparam>
    public static async Task AddNewHistoryEntry<THistory>(
        this DbSet<THistory> history,
        Expression<Func<THistory, bool>> historySelector,
        Func<THistory?, THistory> createNewHistory,
        long transactionStateVersion,
        CancellationToken cancellationToken
    )
        where THistory : HistoryBase
    {
        var existingHistoryItem = history
            .Local
            .Where(historySelector.Compile())
            .FirstOrDefault(h => h.ToStateVersion == null)
            ?? await history
            .Where(historySelector)
            .FirstOrDefaultAsync(h => h.ToStateVersion == null, cancellationToken);

        if (existingHistoryItem == null)
        {
            var newHistoryItem = createNewHistory(null);
            newHistoryItem.FromStateVersion = transactionStateVersion;
            history.Add(newHistoryItem);
        }
        else
        {
            var newHistoryItem = createNewHistory(existingHistoryItem);
            existingHistoryItem.ToStateVersion = transactionStateVersion - 1;
            newHistoryItem.FromStateVersion = transactionStateVersion;
            history.Add(newHistoryItem);
        }
    }
}
