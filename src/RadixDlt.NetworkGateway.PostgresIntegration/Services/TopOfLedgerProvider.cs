using Microsoft.EntityFrameworkCore;
using RadixDlt.NetworkGateway.DataAggregator.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Services;

internal sealed class TopOfLedgerProvider : ITopOfLedgerProvider
{
    private readonly Abstractions.IClock _clock;
    private readonly INetworkConfigurationProvider _networkConfigurationProvider;
    private readonly IDbContextFactory<ReadOnlyDbContext> _dbContextFactory;

    public TopOfLedgerProvider(Abstractions.IClock clock, INetworkConfigurationProvider networkConfigurationProvider, IDbContextFactory<ReadOnlyDbContext> dbContextFactory)
    {
        _clock = clock;
        _networkConfigurationProvider = networkConfigurationProvider;
        _dbContextFactory = dbContextFactory;
    }

    public async Task<TransactionSummary> GetTopOfLedger(CancellationToken token)
    {
        var dbContext = await _dbContextFactory.CreateDbContextAsync(token);

        var lastTransaction = await dbContext.GetTopLedgerTransaction().FirstOrDefaultAsync(token);

        return lastTransaction == null
            ? PreGenesisTransactionSummary()
            : new TransactionSummary(
                StateVersion: lastTransaction.StateVersion,
                TransactionTreeHash: lastTransaction.LedgerHashes.TransactionTreeHash,
                ReceiptTreeHash: lastTransaction.LedgerHashes.ReceiptTreeHash,
                StateTreeHash: lastTransaction.LedgerHashes.StateTreeHash,
                RoundTimestamp: lastTransaction.RoundTimestamp,
                NormalizedRoundTimestamp: lastTransaction.NormalizedRoundTimestamp,
                CreatedTimestamp: lastTransaction.CreatedTimestamp,
                Epoch: lastTransaction.Epoch,
                RoundInEpoch: lastTransaction.RoundInEpoch,
                IndexInEpoch: lastTransaction.IndexInEpoch,
                IndexInRound: lastTransaction.IndexInRound
            );
    }

    private TransactionSummary PreGenesisTransactionSummary()
    {
        // Nearly all of theses turn out to be unused!
        return new TransactionSummary(
            StateVersion: 0,
            TransactionTreeHash: string.Empty,
            ReceiptTreeHash: string.Empty,
            StateTreeHash: string.Empty,
            RoundTimestamp: DateTimeOffset.FromUnixTimeSeconds(0).UtcDateTime,
            NormalizedRoundTimestamp: DateTimeOffset.FromUnixTimeSeconds(0).UtcDateTime,
            CreatedTimestamp: _clock.UtcNow,
            Epoch: _networkConfigurationProvider.GetGenesisEpoch(),
            RoundInEpoch: _networkConfigurationProvider.GetGenesisRound(),
            IndexInEpoch: -1, // invalid, but we increase it by one to in ProcessTransactions
            IndexInRound: -1 // invalid, but we increase it by one to in ProcessTransactions
        );
    }
}
