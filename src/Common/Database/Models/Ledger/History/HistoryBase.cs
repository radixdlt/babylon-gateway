using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Database.Models.Ledger.History;

/// <summary>
/// A base class for History tracked in the database. Current state is given by ToStateVersion = null.
/// </summary>
public abstract class HistoryBase
{
    /// <summary>
    /// The first state version where this version of history applied.
    /// </summary>
    [Column(name: "from_state_version")]
    public long FromStateVersion { get; set; }

    /// <summary>
    /// The last state version where this version of history applied. This endpoint is inclusive.
    /// IE there should be a new History with New.FromStateVersion = Prev.ToStateVersion + 1.
    /// </summary>
    [Column(name: "to_state_version")]
    [ConcurrencyCheck]
    public long? ToStateVersion { get; set; }
}
