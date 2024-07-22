using Dapper;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Services;

internal static class DapperWrapperExtensions
{
    public static async Task<IList<T>> ToList<T>(this IDapperWrapper dapperWrapper, DbContext dbContext, [StringSyntax("postgresql")] string sql, object parameters, CancellationToken token = default)
    {
        var commandDefinition = new CommandDefinition(commandText: sql, parameters: parameters, cancellationToken: token);

        var result = await dapperWrapper.QueryAsync<T>(dbContext.Database.GetDbConnection(), commandDefinition);

        return result.ToList();
    }
}
