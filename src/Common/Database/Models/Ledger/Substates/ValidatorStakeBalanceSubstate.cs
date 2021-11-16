using Common.Numerics;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Database.Models.Ledger.Substates;

/// <summary>
/// <para>
/// UTXOs related to a Validator's stake balance. The Amount is the total staked XRD for that validator.
/// These are written at the end of each epoch for any validators with changes (stakes/unstakes/emissions/data changes).
/// </para>
/// <para>
/// NOTE: Whilst ValidatorStakeData is written to the ledger as a single substate combining Data and Operations,
///       we split it when we store it in the database.
/// </para>
/// </summary>
[Index(nameof(EndOfEpoch), nameof(ValidatorAddress), IsUnique = true)]
[Index(nameof(ValidatorAddress))]
[Table("validator_stake_balance_substates")]
public class ValidatorStakeBalanceSubstate : BalanceSubstateBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidatorStakeBalanceSubstate"/> class.
    /// The SubstateBase properties should be set separately.
    /// </summary>
    public ValidatorStakeBalanceSubstate(string validatorAddress, long endOfEpoch, TokenAmount xrdStakeBalance)
    {
        ValidatorAddress = validatorAddress;
        EndOfEpoch = endOfEpoch;
        Amount = xrdStakeBalance;
    }

    private ValidatorStakeBalanceSubstate()
    {
    }

    [Column(name: "validator_address")]
    public string ValidatorAddress { get; set; }

    [Column(name: "epoch")]
    public long EndOfEpoch { get; set; }
}
