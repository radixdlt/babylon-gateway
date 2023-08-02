using System;
using System.Threading;

namespace RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension;

internal class UnitOfWork
{
    public ReferencedEntityDictionary ReferencedEntities { get; }

    public SequencesHolder Sequences { get; }

    public ReadHelper ReadHelper { get; }

    public CancellationToken CancellationToken { get; }

    public TimeSpan DbReadDuration { get; private set; } = TimeSpan.Zero;

    public TimeSpan DbWriteDuration { get; private set; } = TimeSpan.Zero;

    public UnitOfWork(ReferencedEntityDictionary referencedEntities, SequencesHolder sequences, ReadHelper readHelper, CancellationToken token)
    {
        ReferencedEntities = referencedEntities;
        Sequences = sequences;
        ReadHelper = readHelper;
        CancellationToken = token;
    }

    public void MeasureDbRead(TimeSpan duration)
    {
        DbReadDuration += duration;
    }

    public void MeasureDbWrite(TimeSpan duration)
    {
        DbWriteDuration += duration;
    }
}
