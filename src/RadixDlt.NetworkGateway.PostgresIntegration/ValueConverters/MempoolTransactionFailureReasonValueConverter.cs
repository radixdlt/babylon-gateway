using RadixDlt.NetworkGateway.Commons.Model;
using System.Collections.Generic;

namespace RadixDlt.NetworkGateway.PostgresIntegration.ValueConverters;

internal class MempoolTransactionFailureReasonValueConverter : EnumTypeValueConverterBase<MempoolTransactionFailureReason>
{
    private static readonly Dictionary<MempoolTransactionFailureReason, string> _conversion = new()
    {
        { MempoolTransactionFailureReason.DoubleSpend, "DOUBLE_SPEND" },
        { MempoolTransactionFailureReason.Timeout, "TIMEOUT" },
        { MempoolTransactionFailureReason.Unknown, "UNKNOWN" },
    };

    public MempoolTransactionFailureReasonValueConverter()
        : base(_conversion, Invert(_conversion))
    {
    }
}
