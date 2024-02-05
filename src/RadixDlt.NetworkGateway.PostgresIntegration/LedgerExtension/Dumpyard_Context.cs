using System.Threading;

namespace RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension;

internal record class Dumpyard_Context(
    SequencesHolder Sequences,
    ReadHelper ReadHelper,
    WriteHelper WriteHelper,
    CancellationToken Token);
