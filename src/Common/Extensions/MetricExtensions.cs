using Prometheus;

namespace Common.Extensions;

public enum MetricStatus
{
    Yes,
    Unknown,
    No,
}

public static class MetricExtensions
{
    public static void SetStatus(this Gauge metric, MetricStatus status)
    {
        metric.Set(GetMetricValueForStatus(status));
    }

    public static void SetStatus(this Gauge.Child metric, MetricStatus status)
    {
        metric.Set(GetMetricValueForStatus(status));
    }

    private static double GetMetricValueForStatus(MetricStatus status)
    {
        return status switch
        {
            MetricStatus.Yes => 1,
            MetricStatus.Unknown => 0.5,
            MetricStatus.No => 0,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unknown metric status value"),
        };
    }
}
