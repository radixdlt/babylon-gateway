using Dapper;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Services;

internal static class DapperExtensions
{
    public static CommandDefinition CreateCommandDefinition(
        [StringSyntax("PostgreSQL")]string commandText,
        object? parameters = null,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null,
        CommandFlags flags = CommandFlags.Buffered,
        CancellationToken cancellationToken = default)
    {
        return new CommandDefinition(commandText, parameters, transaction, commandTimeout, commandType, flags, cancellationToken);
    }
}
