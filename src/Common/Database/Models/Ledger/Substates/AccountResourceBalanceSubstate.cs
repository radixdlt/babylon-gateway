using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Database.Models.Ledger.Substates;

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
}
