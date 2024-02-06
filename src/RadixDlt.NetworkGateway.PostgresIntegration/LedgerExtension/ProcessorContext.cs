using System.Threading;

namespace RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension;

internal record ProcessorContext(SequencesHolder Sequences, ReadHelper ReadHelper, WriteHelper WriteHelper, CancellationToken Token);
