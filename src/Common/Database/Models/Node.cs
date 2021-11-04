using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Database.Models;

/// <summary>
/// A Node definition that the DataAggregator knows (or knew) about.
/// This table is regularly kept updated from the DataAggregator's configuration.
/// </summary>
[Table("nodes")]
public class Node
{
    [Key]
    [Column(name: "name")]
    public string Name { get; set; }

    [Column(name: "address")]
    public string Address { get; set; }

    [Column(name: "trust_weighting")]
    public decimal TrustWeighting { get; set; }

    [Column(name: "enabled_for_indexing")]
    public bool EnabledForIndexing { get; set; }

    public Node(string name, string address, decimal trustWeighting, bool enabledForIndexing)
    {
        Name = name;
        Address = address;
        TrustWeighting = trustWeighting;
        EnabledForIndexing = enabledForIndexing;
    }
}
