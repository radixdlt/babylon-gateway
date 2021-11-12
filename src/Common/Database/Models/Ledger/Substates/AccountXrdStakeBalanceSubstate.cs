using Common.Database.ValueConverters;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Database.Models.Ledger.Substates;

public enum AccountXrdStakeBalanceSubstateType
{
    PreparingStake,
    ExitingStake,
}

public class AccountXrdStakeBalanceSubstateTypeValueConverter : EnumTypeValueConverterBase<AccountXrdStakeBalanceSubstateType>
{
    private static Dictionary<AccountXrdStakeBalanceSubstateType, string> _conversion = new()
    {
        { AccountXrdStakeBalanceSubstateType.PreparingStake, "PREPARING_STAKE" },
        { AccountXrdStakeBalanceSubstateType.ExitingStake, "EXITING_STAKE" },
    };

    public AccountXrdStakeBalanceSubstateTypeValueConverter()
        : base(_conversion, Invert(_conversion))
    {
    }
}

/// <summary>
/// UTXOs related to Accounts staking to Validators, where the resource is XRD.
/// In particular, this is PreparingStake and ExitingStake.
/// Only ExitingStake has an UnlockEpoch - PreparingStake will take effect at the end of the current epoch.
/// </summary>
[Index(nameof(AccountAddress), nameof(ValidatorAddress))]
[Index(nameof(ValidatorAddress), nameof(AccountAddress))]
[Table("account_xrd_stake_balance_substates")]
public class AccountXrdStakeBalanceSubstate : BalanceSubstateBase
{
    [Column(name: "account_address")]
    public string AccountAddress { get; set; }

    [Column(name: "validator_address")]
    public string ValidatorAddress { get; set; }

    [Column(name: "type")]
    public AccountXrdStakeBalanceSubstateType Type { get; set; }

    [Column(name: "unlock_epoch")]
    public long? UnlockEpoch { get; set; }
}
