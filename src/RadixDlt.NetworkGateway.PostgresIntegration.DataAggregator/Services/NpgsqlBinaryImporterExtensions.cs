using Npgsql;
using NpgsqlTypes;
using System.Threading;
using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Services;

internal static class NpgsqlBinaryImporterExtensions
{
    public static Task WriteNullableAsync(this NpgsqlBinaryImporter writer, long? value, NpgsqlDbType npgsqlDbType, CancellationToken cancellationToken = default)
    {
        return value.HasValue
            ? writer.WriteAsync(value.Value, npgsqlDbType, cancellationToken)
            : writer.WriteNullAsync(cancellationToken);
    }

    public static Task WriteNullableAsync(this NpgsqlBinaryImporter writer, byte[]? value, NpgsqlDbType npgsqlDbType, CancellationToken cancellationToken = default)
    {
        return value != null
            ? writer.WriteAsync(value, npgsqlDbType, cancellationToken)
            : writer.WriteNullAsync(cancellationToken);
    }
}
