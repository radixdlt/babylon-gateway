namespace RadixDlt.NetworkGateway.Common.Model;

public enum MempoolTransactionFailureReason
{
    DoubleSpend,
    Timeout,
    Unknown,
    // Invalid shouldn't be possible, because they shouldn't make it to this table in the first place - mark as Unknown
}
