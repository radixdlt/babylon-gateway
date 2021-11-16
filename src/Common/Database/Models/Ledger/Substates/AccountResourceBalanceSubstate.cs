using Common.Database.Models.Ledger.History;
using Common.Numerics;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Database.Models.Ledger.Substates;

public record struct AccountResource(string AccountAddress, string ResourceIdentifier);

/// <summary>
/// UTXOs related to Account Resource Balances.
/// </summary>
[Index(nameof(AccountAddress), nameof(ResourceIdentifier))]
[Index(nameof(ResourceIdentifier), nameof(AccountAddress))]
[Table("account_resource_balance_substates")]
public class AccountResourceBalanceSubstate : BalanceSubstateBase
{
    [Column(name: "account_address")]
    public string AccountAddress { get; set; }

    [Column(name: "rri")]
    public string? ResourceIdentifier { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AccountResourceBalanceSubstate"/> class.
    /// The SubstateBase properties should be set separately.
    /// </summary>
    public AccountResourceBalanceSubstate(AccountResource key, TokenAmount amount)
    {
        AccountAddress = key.AccountAddress;
        ResourceIdentifier = key.ResourceIdentifier;
        Amount = amount;
    }

    private AccountResourceBalanceSubstate()
    {
    }
}
