using Common.Database.Models.Ledger.Substates;
using Common.Numerics;
using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Database.Models.Ledger.History;

public record struct BalanceEntry(TokenAmount Balance);

/// <summary>
/// UTXOs related to Account Resource Balances.
/// </summary>
// OnModelCreating: Indexes defined there.
// OnModelCreating: Composite primary key is defined there.
[Table("account_resource_balance_history")]
public class AccountResourceBalanceHistory : HistoryBase<AccountResource, BalanceEntry>
{
    [Column(name: "account_address")]
    public string AccountAddress { get; set; }

    [Column(name: "rri")]
    public string ResourceIdentifier { get; set; }

    [Column(name: "balance")]
    public TokenAmount Balance { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AccountResourceBalanceHistory"/> class.
    /// The StateVersions should be set separately.
    /// </summary>
    public AccountResourceBalanceHistory(AccountResource key, TokenAmount startingBalance)
    {
        AccountAddress = key.AccountAddress;
        ResourceIdentifier = key.ResourceIdentifier;
        Balance = startingBalance;
    }

    public static AccountResourceBalanceHistory FromPreviousEntry(
        AccountResource key,
        BalanceEntry previousBalance,
        TokenAmount balanceChange
    )
    {
        return new AccountResourceBalanceHistory(key, previousBalance.Balance + balanceChange);
    }

    public static AccountResourceBalanceHistory FromPreviousHistory(
        AccountResource key,
        AccountResourceBalanceHistory? previousHistory,
        TokenAmount balanceChange
    )
    {
        return previousHistory == null
            ? new AccountResourceBalanceHistory(key, balanceChange)
            : FromPreviousEntry(key, previousHistory.GetEntry(), balanceChange);
    }

    private AccountResourceBalanceHistory()
    {
    }

    public override bool Matches(AccountResource key)
    {
        return ResourceIdentifier == key.ResourceIdentifier &&
               AccountAddress == key.AccountAddress;
    }

    public override BalanceEntry GetEntry()
    {
        return new BalanceEntry(Balance);
    }
}
