using Microsoft.EntityFrameworkCore;
using RadixDlt.NetworkGateway.GatewayApi.Services;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Services;

internal class OpenApiDocumentQuerier : IOpenApiDocumentQuerier
{
    private readonly ReadOnlyDbContext _dbContext;

    public OpenApiDocumentQuerier(ReadOnlyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<OpenApiDocumentPlaceholderData> GetPlaceholderData(CancellationToken token = default)
    {
        var randomIntentHash = await _dbContext
            .LedgerTransactions
            .OfType<BaseUserLedgerTransaction>()
            .Select(x => x.IntentHash)
            .AnnotateMetricName("RandomIntentHash")
            .FirstOrDefaultAsync(token);

        var randomSubintentHash = await _dbContext
            .LedgerFinalizedSubintents
            .Select(x => x.SubintentHash)
            .AnnotateMetricName("RandomSubintentHash")
            .FirstOrDefaultAsync(token);

        var currentEpoch = await _dbContext
            .LedgerTransactions
            .OrderByDescending(x => x.StateVersion)
            .Select(x => x.Epoch)
            .AnnotateMetricName("CurrentEpoch")
            .FirstOrDefaultAsync(token);

        var requirement = await _dbContext
            .EntitiesByRoleRequirement
            .OfType<EntitiesByNonFungibleRoleRequirement>()
            .OrderByDescending(x => x.FirstSeenStateVersion)
            .Join(
                _dbContext.Entities,
                x => x.ResourceEntityId,
                y => y.Id,
                (requirement, entity) => new { entity.Address, requirement.NonFungibleLocalId }
            )
            .FirstOrDefaultAsync(token);

        return new OpenApiDocumentPlaceholderData(randomIntentHash, randomSubintentHash, currentEpoch, requirement?.Address, requirement?.NonFungibleLocalId);
    }
}
