using Common.Database.ValueConverters;
using Common.Numerics;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Database.Models.Ledger.Substates;

public enum AccountStakeOwnershipBalanceSubstateType
{
    Stake,
    PreparedUnstake,
}

public class AccountStakeOwnershipBalanceSubstateTypeValueConverter : EnumTypeValueConverterBase<AccountStakeOwnershipBalanceSubstateType>
{
    private static Dictionary<AccountStakeOwnershipBalanceSubstateType, string> _conversion = new()
    {
        { AccountStakeOwnershipBalanceSubstateType.Stake, "STAKE" },
        { AccountStakeOwnershipBalanceSubstateType.PreparedUnstake, "PREPARED_UNSTAKE" },
    };

    public AccountStakeOwnershipBalanceSubstateTypeValueConverter()
        : base(_conversion, Invert(_conversion))
    {
    }
}

/// <summary>
/// UTXOs related to Accounts staking to Validators, where the resource is a share of StakeOwnership in that Validator.
/// In particular, this is stake which is Staked or PreparingUnstake.
/// </summary>
[Index(nameof(AccountAddress), nameof(ValidatorAddress))]
[Index(nameof(ValidatorAddress), nameof(AccountAddress))]
[Table("account_stake_ownership_balance_substates")]
public class AccountStakeOwnershipBalanceSubstate : BalanceSubstateBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AccountStakeOwnershipBalanceSubstate"/> class.
    /// The SubstateBase properties should be set separately.
    /// </summary>
    public AccountStakeOwnershipBalanceSubstate(
        string accountAddress,
        string validatorAddress,
        AccountStakeOwnershipBalanceSubstateType type,
        TokenAmount stakeOwnershipBalance
    )
    {
        AccountAddress = accountAddress;
        ValidatorAddress = validatorAddress;
        Type = type;
        Amount = stakeOwnershipBalance;
    }

    private AccountStakeOwnershipBalanceSubstate()
    {
    }

    [Column(name: "account_address")]
    public string AccountAddress { get; set; }

    [Column(name: "validator_address")]
    public string ValidatorAddress { get; set; }

    [Column(name: "type")]
    public AccountStakeOwnershipBalanceSubstateType Type { get; set; }
}
