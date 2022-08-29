using RadixDlt.NetworkGateway.Common.Model;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace RadixDlt.NetworkGateway.PostgresIntegration.ValueConverters;

internal class MempoolTransactionStatusValueConverter : EnumTypeValueConverterBase<MempoolTransactionStatus>
{
    public static readonly ImmutableDictionary<MempoolTransactionStatus, string> Conversion =
        new Dictionary<MempoolTransactionStatus, string>()
        {
            { MempoolTransactionStatus.SubmittedOrKnownInNodeMempool, "IN_NODE_MEMPOOL" },
            { MempoolTransactionStatus.Missing, "MISSING" },
            { MempoolTransactionStatus.ResolvedButUnknownTillSyncedUp, "RESOLVED_BUT_UNKNOWN_TILL_SYNCED_UP" },
            { MempoolTransactionStatus.Failed, "FAILED" },
            { MempoolTransactionStatus.Committed, "COMMITTED" },
        }.ToImmutableDictionary();

    public MempoolTransactionStatusValueConverter()
        : base(Conversion, Invert(Conversion))
    {
    }
}
