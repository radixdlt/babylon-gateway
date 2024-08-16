using RadixDlt.NetworkGateway.PostgresIntegration.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Queries;

internal static class NonFungibleVaultContentsQuery
{
    public record struct QueryConfiguration(int NonFungibleIdsPerVault, long AtLedgerState);

    public static async Task<Dictionary<long, string[]>> Execute(
        ReadOnlyDbContext dbContext,
        IDapperWrapper dapperWrapper,
        ICollection<long> vaultIds,
        QueryConfiguration configuration,
        CancellationToken token = default)
    {
        await Task.CompletedTask;
// start with:
//         var cd = dapperWrapper.CreateCommandDefinition(
//             @"WITH
// variables AS (
//     SELECT
//         unnest(@vaultIds) AS vault_id,
//         @nonFungibleIdsPerVault AS non_fungible_ids_per_vault,
//         @atLedgerState AS at_ledger_state
// ),
// non_fungible_ids_per_vault AS (
//
// )
// SELECT * FROM non_fungible_ids_per_vault",
//             new
//             {
//                 vaultIds = vaultIds.ToList(),
//                 nonFungibleIdsPerVault = configuration.NonFungibleIdsPerVault + 1,
//                 atLedgerState = configuration.AtLedgerState,
//             },
//             token);

        return vaultIds.ToDictionary(id => id, _ =>
        {
            return Enumerable.Range(0, configuration.NonFungibleIdsPerVault + 1).Select(i => $"TBD TBD #{i + 1}").ToArray();
        });
    }
}
