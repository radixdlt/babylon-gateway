using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Threading.Tasks;

namespace GatewayApi.SlowRequestLogging;

internal sealed class SlowRequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger _logger;
    private readonly IOptionsMonitor<SlowRequestLoggingOptions> _options;

    public SlowRequestLoggingMiddleware(RequestDelegate next, IOptionsMonitor<SlowRequestLoggingOptions> options, ILogger<SlowRequestLoggingMiddleware> logger)
    {
        _next = next;
        _options = options;
        _logger = logger;
    }

    public Task Invoke(HttpContext context)
    {
        if (!_logger.IsEnabled(LogLevel.Information))
        {
            // Logger isn't enabled.
            return _next(context);
        }

        return InvokeInternal(context);
    }

    private async Task InvokeInternal(HttpContext context)
    {
        var options = _options.CurrentValue;
        var request = context.Request;
        var requestBodyLogLimit = options.RequestBodyLogLimit;
        var encoding = options.Encoding;
        var originalBody = request.Body;
        var requestBufferingStream = new RequestBufferingStream(request.Body, requestBodyLogLimit, _logger, encoding);

        request.Body = requestBufferingStream;

        try
        {
            var ts = Stopwatch.GetTimestamp();

            await _next(context);

            var elapsed = Stopwatch.GetElapsedTime(ts);

            if (elapsed > options.SlowRequestThreshold)
            {
                var requestBody = requestBufferingStream.GetString(encoding);

                _logger.LogWarning(
                    "{Method} {Path}{QueryString} took {Elapsed}ms to complete. Request body: {RequestBody}",
                    request.Method, request.Path, request.QueryString, (int)elapsed.TotalMilliseconds, requestBody);
            }
        }
        finally
        {
            await requestBufferingStream.DisposeAsync();

            context.Request.Body = originalBody;
        }
    }
}
