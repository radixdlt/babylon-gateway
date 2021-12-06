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

using Common.Database.Models.Ledger.Normalization;
using Common.Numerics;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;

namespace Common.Database.Models.Ledger.History;

/// <summary>
/// Tracks validator stake over time.
/// </summary>
// OnModelCreating: Indexes defined there.
// OnModelCreating: Composite primary key is defined there.
[Table("validator_stake_history")]
public class ValidatorStakeHistory : HistoryBase<Validator, ValidatorStakeSnapshot, ValidatorStakeSnapshotChange>
{
    [Column(name: "validator_id")]
    public long ValidatorId { get; set; }

    [ForeignKey(nameof(ValidatorId))]
    public Validator Validator { get; set; }

    // [Owned] below
    public ValidatorStakeSnapshot StakeSnapshot { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidatorStakeHistory"/> class.
    /// The StateVersions should be set separately.
    /// </summary>
    public ValidatorStakeHistory(Validator key, ValidatorStakeSnapshot stakeSnapshot)
    {
        Validator = key;
        StakeSnapshot = stakeSnapshot;
    }

    public static ValidatorStakeHistory FromPreviousEntry(
        Validator key,
        ValidatorStakeSnapshot? previous,
        ValidatorStakeSnapshotChange change
    )
    {
        var prev = previous ?? ValidatorStakeSnapshot.GetDefault();
        return new ValidatorStakeHistory(key, prev.CreateNewFromChange(change));
    }

    private ValidatorStakeHistory()
    {
    }
}

/// <summary>
/// A mutable class to aggregate changes.
/// </summary>
public class ValidatorStakeSnapshotChange
{
    public TokenAmount ChangeInXrdStake { get; set; }

    public TokenAmount ChangeInStakeUnits { get; set; }

    public TokenAmount ChangeInPreparedXrdStake { get; set; }

    public TokenAmount ChangeInPreparedUnStakeUnits { get; set; }

    public TokenAmount ChangeInExitingXrdStake { get; set; }

    public static ValidatorStakeSnapshotChange Default()
    {
        return new ValidatorStakeSnapshotChange();
    }

    public bool IsMeaningfulChange()
    {
        return !(
            ChangeInXrdStake.IsZero() &&
            ChangeInStakeUnits.IsZero() &&
            ChangeInPreparedXrdStake.IsZero() &&
            ChangeInPreparedUnStakeUnits.IsZero() &&
            ChangeInExitingXrdStake.IsZero()
        );
    }

    public bool IsNaN()
    {
        return ChangeInXrdStake.IsNaN() ||
               ChangeInStakeUnits.IsNaN() ||
               ChangeInPreparedXrdStake.IsNaN() ||
               ChangeInPreparedUnStakeUnits.IsNaN() ||
               ChangeInExitingXrdStake.IsNaN();
    }

    public void AggregateXrdStakeChange(TokenAmount change)
    {
        ChangeInXrdStake += change;
    }

    public void AggregateStakeUnitChange(TokenAmount change)
    {
        ChangeInStakeUnits += change;
    }

    public void AggregatePreparedXrdStakeChange(TokenAmount change)
    {
        ChangeInPreparedXrdStake += change;
    }

    public void AggregatePreparedUnStakeUnitChange(TokenAmount change)
    {
        ChangeInPreparedUnStakeUnits += change;
    }

    public void AggregateChangeInExitingXrdStakeChange(TokenAmount change)
    {
        ChangeInExitingXrdStake += change;
    }
}

[Owned]
public record ValidatorStakeSnapshot
{
    [Column("total_xrd_staked")]
    public TokenAmount TotalXrdStake { get; set; }

    [Column("total_stake_ownership")]
    public TokenAmount TotalStakeUnits { get; set; }

    [Column("total_prepared_xrd_stake")]
    public TokenAmount TotalPreparedXrdStake { get; set; }

    [Column("total_prepared_unstake_ownership")]
    public TokenAmount TotalPreparedUnStakeUnits { get; set; }

    [Column("total_exiting_xrd_stake")]
    public TokenAmount TotalExitingXrdStake { get; set; }

    public static ValidatorStakeSnapshot GetDefault()
    {
        return new ValidatorStakeSnapshot
        {
            TotalXrdStake = TokenAmount.Zero,
            TotalStakeUnits = TokenAmount.Zero,
            TotalPreparedXrdStake = TokenAmount.Zero,
            TotalPreparedUnStakeUnits = TokenAmount.Zero,
            TotalExitingXrdStake = TokenAmount.Zero,
        };
    }

    public ValidatorStakeSnapshot CreateNewFromChange(ValidatorStakeSnapshotChange change)
    {
        return new ValidatorStakeSnapshot
        {
            TotalXrdStake = TotalXrdStake + change.ChangeInXrdStake,
            TotalStakeUnits = TotalStakeUnits + change.ChangeInStakeUnits,
            TotalPreparedXrdStake = TotalPreparedXrdStake + change.ChangeInPreparedXrdStake,
            TotalPreparedUnStakeUnits = TotalPreparedUnStakeUnits + change.ChangeInPreparedUnStakeUnits,
            TotalExitingXrdStake = TotalExitingXrdStake + change.ChangeInExitingXrdStake,
        };
    }

    public TokenAmount EstimateXrdConversion(TokenAmount stakeUnits)
    {
        var totalEffectiveStakeUnits = TotalPreparedUnStakeUnits + TotalStakeUnits;

        if (totalEffectiveStakeUnits.IsZero())
        {
            return TokenAmount.Zero;
        }

        if (stakeUnits >= totalEffectiveStakeUnits)
        {
            return TotalXrdStake;
        }

        return (stakeUnits * TotalXrdStake) / totalEffectiveStakeUnits;
    }
}
