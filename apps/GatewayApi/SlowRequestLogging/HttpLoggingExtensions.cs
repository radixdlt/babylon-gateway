// <auto-generated>
#nullable enable

using Microsoft.Extensions.Logging;
using System;

namespace GatewayApi.SlowRequestLogging;

internal static partial class HttpLoggingExtensions
{
    public static void RequestLog(this ILogger logger, HttpRequestLog requestLog) => logger.Log(
        LogLevel.Information,
        new EventId(1, "RequestLog"),
        requestLog,
        exception: null,
        formatter: HttpRequestLog.Callback);

    [LoggerMessage(3, LogLevel.Information, "RequestBody: {Body}", EventName = "RequestBody")]
    public static partial void SlowRequest(this ILogger logger, string body);

    [LoggerMessage(5, LogLevel.Debug, "Decode failure while converting body.", EventName = "DecodeFailure")]
    public static partial void DecodeFailure(this ILogger logger, Exception ex);

    [LoggerMessage(6, LogLevel.Debug, "Unrecognized Content-Type for {Name} body.", EventName = "UnrecognizedMediaType")]
    public static partial void UnrecognizedMediaType(this ILogger logger, string name);

    [LoggerMessage(7, LogLevel.Debug, "No Content-Type header for {Name} body.", EventName = "NoMediaType")]
    public static partial void NoMediaType(this ILogger logger, string name);
}