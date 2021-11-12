using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Database.Models.Ledger.Substates;

/// <summary>
/// <para>
/// Epochs (which also coincidentally are System EpochData UTXOs)
/// These are downed/upped in the last transaction of each epoch.
/// </para>
/// </summary>
[Table("epochs")]
public class EpochSubstate : DataSubstateBase
{
    [Column(name: "epoch")]
    public long EpochNumber { get; set; }
}
