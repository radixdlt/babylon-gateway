using Common.Numerics;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Database.Models.Ledger.History;

/// <summary>
/// UTXOs related to Account Resource Balances.
/// </summary>
// OnModelCreating: Indexes defined there.
// OnModelCreating: Composite primary key is defined there.
[Table("account_resource_balance_history")]
public class AccountResourceBalanceHistory : HistoryBase
{
    [Column(name: "account_address")]
    public string AccountAddress { get; set; }

    [Column(name: "rri")]
    public string ResourceIdentifier { get; set; }

    [Column(name: "balance")]
    public TokenAmount Balance { get; set; }
}
