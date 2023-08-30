using System;
using System.Text;

namespace GatewayApi.SlowRequestLogging;

public sealed class SlowRequestLoggingOptions
{
    public int RequestBodyLogLimit { get; set; } = 1 * 1024;

    public TimeSpan SlowRequestThreshold { get; set; } = TimeSpan.FromMilliseconds(500);

    public Encoding Encoding { get; set; } = Encoding.UTF8;
}
