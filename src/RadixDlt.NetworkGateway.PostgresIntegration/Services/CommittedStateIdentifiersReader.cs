using Microsoft.EntityFrameworkCore;
using RadixDlt.NetworkGateway.DataAggregator.Services;
using System.Threading;
using System.Threading.Tasks;
using GatewayModel = RadixDlt.NetworkGateway.Abstractions;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Services;

internal sealed class CommittedStateIdentifiersReader : ICommittedStateIdentifiersReader
{
    private readonly IDbContextFactory<ReadOnlyDbContext> _dbContextFactory;

    public CommittedStateIdentifiersReader(IDbContextFactory<ReadOnlyDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<GatewayModel.CommittedStateIdentifiers?> GetStateIdentifiersForStateVersion(long stateVersion, CancellationToken cancellationToken)
    {
        var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var transaction = await dbContext.LedgerTransactions
            .FirstOrDefaultAsync(x => x.StateVersion == stateVersion, cancellationToken);

        if (transaction == null)
        {
            return null;
        }

        return new GatewayModel.CommittedStateIdentifiers(
            transaction.StateVersion,
            transaction.LedgerHashes.StateTreeHash,
            transaction.LedgerHashes.TransactionTreeHash,
            transaction.LedgerHashes.ReceiptTreeHash);
    }
}
