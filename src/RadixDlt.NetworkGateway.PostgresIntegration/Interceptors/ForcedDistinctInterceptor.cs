using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Interceptors;

internal class ForceDistinctInterceptor : DbCommandInterceptor
{
    /// <summary>Query marked used to trigger interceptor.</summary>
    /// <remarks>Value has no meaning at all, it should be understood as opaque query marker.</remarks>
    public const string Apply = nameof(ForceDistinctInterceptor) + ":3c49f785-0598-462a-ba88-bcdbed969709-f66acff3-fd40-4fbd-8eb9-151aefcc5711";

    public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
    {
        ModifyCommand(command);

        return result;
    }

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
    {
        ModifyCommand(command);

        return new ValueTask<InterceptionResult<DbDataReader>>(result);
    }

    private static void ModifyCommand(DbCommand command)
    {
        // TODO something slightly more sophisticated needed as this is going to fail if multiple tags were applied or query with more than one SELECT clause was used
        if (command.CommandText.StartsWith($"-- {Apply}", StringComparison.Ordinal))
        {
            command.CommandText = command.CommandText.Replace("SELECT ", "SELECT DISTINCT ");
        }
    }
}
