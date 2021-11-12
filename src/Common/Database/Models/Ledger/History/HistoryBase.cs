using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Database.Models.Ledger.History;

public abstract class HistoryBase
{
    /// <summary>
    ///  The first state version where this version of history applied.
    /// </summary>
    [Column(name: "from_state_version")]
    public long FromStateVersion { get; set; }

    /// <summary>
    ///  The last state version where this version of history applied. This endpoint is inclusive.
    ///  IE there should be a new History with New.FromStateVersion = Prev.ToStateVersion + 1.
    /// </summary>
    [Column(name: "to_state_version")]
    public long? ToStateVersion { get; set; }
}
