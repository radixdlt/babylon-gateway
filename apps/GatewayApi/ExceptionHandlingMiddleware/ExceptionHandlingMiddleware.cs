/* Copyright 2021 Radix Publishing Ltd incorporated in Jersey (Channel Islands).
 *
 * Licensed under the Radix License, Version 1.0 (the "License"); you may not use this
 * file except in compliance with the License. You may obtain a copy of the License at:
 *
 * radixfoundation.org/licenses/LICENSE-v1
 *
 * The Licensor hereby grants permission for the Canonical version of the Work to be
 * published, distributed and used under or by reference to the Licensor’s trademark
 * Radix ® and use of any unregistered trade names, logos or get-up.
 *
 * The Licensor provides the Work (and each Contributor provides its Contributions) on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied,
 * including, without limitation, any warranties or conditions of TITLE, NON-INFRINGEMENT,
 * MERCHANTABILITY, or FITNESS FOR A PARTICULAR PURPOSE.
 *
 * Whilst the Work is capable of being deployed, used and adopted (instantiated) to create
 * a distributed ledger it is your responsibility to test and validate the code, together
 * with all logic and performance of that code under all foreseeable scenarios.
 *
 * The Licensor does not make or purport to make and hereby excludes liability for all
 * and any representation, warranty or undertaking in any form whatsoever, whether express
 * or implied, to any entity or person, including any representation, warranty or
 * undertaking, as to the functionality security use, value or other characteristics of
 * any distributed ledger nor in respect the functioning or value of any tokens which may
 * be created stored or transferred using the Work. The Licensor does not warrant that the
 * Work or any use of the Work complies with any law or regulation in any territory where
 * it may be implemented or used or that it will be appropriate for any specific purpose.
 *
 * Neither the licensor nor any current or former employees, officers, directors, partners,
 * trustees, representatives, agents, advisors, contractors, or volunteers of the Licensor
 * shall be liable for any direct or indirect, special, incidental, consequential or other
 * losses of any kind, in tort, contract or otherwise (including but not limited to loss
 * of revenue, income or profits, or loss of use or data, or loss of reputation, or loss
 * of any economic or other opportunity of whatsoever nature or howsoever arising), arising
 * out of or in connection with (without limitation of any use, misuse, of any ledger system
 * or use made or its functionality or any performance or operation of any code or protocol
 * caused by bugs or programming or logic errors or otherwise);
 *
 * A. any offer, purchase, holding, use, sale, exchange or transmission of any
 * cryptographic keys, tokens or assets created, exchanged, stored or arising from any
 * interaction with the Work;
 *
 * B. any failure in a transmission or loss of any token or assets keys or other digital
 * artefacts due to errors in transmission;
 *
 * C. bugs, hacks, logic errors or faults in the Work or any communication;
 *
 * D. system software or apparatus including but not limited to losses caused by errors
 * in holding or transmitting tokens by any third-party;
 *
 * E. breaches or failure of security including hacker attacks, loss or disclosure of
 * password, loss of private key, unauthorised use or misuse of such passwords or keys;
 *
 * F. any losses including loss of anticipated savings or other benefits resulting from
 * use of the Work or any changes to the Work (however implemented).
 *
 * You are solely responsible for; testing, validating and evaluation of all operation
 * logic, functionality, security and appropriateness of using the Work for any commercial
 * or non-commercial purpose and for any reproduction or redistribution by You of the
 * Work. You assume all risks associated with Your use of the Work and the exercise of
 * permissions under this License.
 */

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RadixDlt.NetworkGateway.Abstractions.Extensions;
using RadixDlt.NetworkGateway.GatewayApi.AspNetCore;
using RadixDlt.NetworkGateway.GatewayApi.Exceptions;
using RadixDlt.NetworkGateway.GatewayApi.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using CoreClient = RadixDlt.CoreApiSdk.Client;
using CoreModel = RadixDlt.CoreApiSdk.Model;
using GatewayModel = RadixDlt.NetworkGateway.GatewayApiSdk.Model;

namespace GatewayApi.ExceptionHandlingMiddleware;

internal sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger _logger;
    private readonly IEnumerable<IExceptionObserver> _observers;
    private readonly LogLevel _knownGatewayErrorLogLevel;

    public ExceptionHandlingMiddleware(RequestDelegate next, IHostEnvironment env, ILogger<ExceptionHandlingMiddleware> logger,
        IEnumerable<IExceptionObserver> observers)
    {
        _next = next;
        _logger = logger;
        _observers = observers;
        _knownGatewayErrorLogLevel = env.IsDevelopment() ? LogLevel.Information : LogLevel.Debug;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            var traceId = Activity.Current?.Id ?? context.TraceIdentifier;

            var gatewayErrorException = HandleException(context, exception, traceId);

            var errorResponse = new GatewayModel.ErrorResponse(
                code: gatewayErrorException.StatusCode,
                message: gatewayErrorException.UserFacingMessage,
                details: gatewayErrorException.GatewayError,
                traceId: traceId
            );

            _observers.ForEach(x => x.OnException(context, exception, gatewayErrorException));

            if (context.Response.HasStarted)
            {
                _logger.LogWarning("Response already started. Can't modify status code and response body");
            }
            else
            {
                var jsonErrorResponse = JsonConvert.SerializeObject(errorResponse);
                await WriteResponseAsync(context, jsonErrorResponse, gatewayErrorException.StatusCode);
            }
        }
    }

    private KnownGatewayErrorException HandleException(HttpContext httpContext, Exception exception, string traceId)
    {
        switch (exception)
        {
            case BadHttpRequestException badHttpRequestException :
                _logger.LogInformation(exception, "Bad http request. [RequestTrace={TraceId}]", traceId);
                return new BadRequestException("Bad http request", badHttpRequestException.Message);

            case OperationCanceledException operationCanceledException:
                var requestTimeoutFeature = httpContext.Features.Get<IRequestTimeoutFeature>();
                var requestAbortedFeature = httpContext.Features.Get<IRequestAbortedFeature>();

                if (requestTimeoutFeature?.CancellationToken.IsCancellationRequested == true)
                {
                    _logger.LogError(exception, "Request timed out after={Timeout} seconds, [RequestTrace={TraceId}]", requestTimeoutFeature.TimeoutAfter.TotalSeconds, traceId);
                    return InternalServerException.OfRequestTimeoutException(exception, traceId);
                }

                if (requestAbortedFeature?.CancellationToken.IsCancellationRequested == true)
                {
                    _logger.LogWarning(exception, "Request aborted by user. [RequestTrace={TraceId}]", traceId);
                    return new ClientClosedConnectionException("Client closed connection", "Client closed connection");
                }

                _logger.LogWarning(
                    exception,
                    "Unknown exception [RequestTrace={TraceId}]",
                    traceId
                );

                return InternalServerException.OfHiddenException(exception, traceId);
            case KnownGatewayErrorException knownGatewayErrorException:
                _logger.Log(
                    _knownGatewayErrorLogLevel,
                    exception,
                    "Known exception with http response code [RequestTrace={TraceId}]",
                    traceId
                );
                return knownGatewayErrorException;

            // HttpRequestException is returned from the Core API if we can't connect
            case HttpRequestException:
                _logger.LogInformation(
                    exception,
                    "Error relaying request to upstream server [RequestTrace={TraceId}]",
                    traceId
                );
                return InternalServerException.OfInvalidGatewayException(traceId);

            // CoreClient.ApiException is returned if we get a 500 from upstream but couldn't extract a WrappedCoreApiException
            case CoreClient.ApiException coreApiException:
                _logger.LogWarning(
                    exception,
                    "Unhandled error response from upstream core API, which didn't parse correctly into a known ErrorType we could wrap or unexpected ErrorType [RequestTrace={TraceId}]",
                    traceId
                );
                return InternalServerException.OfUnhandledCoreApiException(
                    coreApiException.ErrorContent?.ToString() ?? string.Empty,
                    traceId
                );

            case InvalidCoreApiResponseException invalidCoreApiResponseException:
                _logger.LogWarning(
                    exception,
                    "Invalid Core API response [RequestTrace={TraceId}]",
                    traceId
                );
                return InternalServerException.OfInvalidCoreApiResponseException(
                    invalidCoreApiResponseException,
                    traceId
                );

            default:
                _logger.LogWarning(
                    exception,
                    "Unknown exception [RequestTrace={TraceId}]",
                    traceId
                );
                return InternalServerException.OfHiddenException(exception, traceId);
        }
    }

    private async Task WriteResponseAsync(HttpContext context, string jsonResponse, int statusCode)
    {
        var response = context.Response;
        response.ContentType = "application/json";
        response.StatusCode = statusCode;
        await response.WriteAsync(jsonResponse);
    }
}
