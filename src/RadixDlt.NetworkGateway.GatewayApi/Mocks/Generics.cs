using System.Collections.Generic;

namespace RadixDlt.NetworkGateway.GatewayApi.Mocks;

/// <summary>
/// Generic cursor-based paginable collection.
/// </summary>
public record ResultSet<T>(ICollection<T> Results, string Cursor, int? Total);
