using Common.Numerics;
using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Database.Models.Ledger.Substates;

public abstract class BalanceSubstateBase : SubstateBase
{
    [Column(name: "amount")]
    public TokenAmount Amount { get; set; }
}
