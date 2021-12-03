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

using GatewayAPI.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.Json;
using CoreApi = RadixCoreApi.GeneratedClient.Client;
using GatewayApi = RadixGatewayApi.Generated.Client;

namespace GatewayAPI.ApiSurface;

public class ExceptionFilter : IActionFilter, IOrderedFilter
{
    private readonly ILogger<ExceptionFilter> _logger;
    private LogLevel _knownErrorLogLevel;

    public ExceptionFilter(ILogger<ExceptionFilter> logger, IHostEnvironment env)
    {
        _logger = logger;
        _knownErrorLogLevel = env.IsDevelopment() ? LogLevel.Information : LogLevel.Debug;
    }

    public int Order => int.MaxValue - 10;

    public void OnActionExecuting(ActionExecutingContext context)
    {
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Exception == null)
        {
            return;
        }

        Exception exception = context.Exception!;
        HttpResponseException outException;

        if (exception is HttpResponseException httpResponseException)
        {
            _logger.Log(_knownErrorLogLevel, exception, "Known exception with http response code");
            outException = httpResponseException;
        }
        else if (exception is HttpRequestException)
        {
            // HttpRequestException is returned from the Gateway or Core APIs if we can't connect
            _logger.Log(LogLevel.Information, exception, "Error relaying request to upstream server");
            outException = new HttpResponseException { Status = 502, ExceptionNameUpperSnakeCase = "BAD_GATEWAY" };
        }
        else if (exception is CoreApi.ApiException coreApiException)
        {
            // CoreApi.ApiException is returned if we get a 500 from upstream
            _logger.Log(LogLevel.Information, exception, "Error response from upstream server");
            var (exceptionName, causeName) = ExtractExceptionAndCause(coreApiException.ErrorContent.ToString() ?? string.Empty);
            outException = new HttpResponseException(causeName)
            {
                Status = coreApiException.ErrorCode,
                ExceptionNameUpperSnakeCase = exceptionName ?? "UNKNOWN_ERROR",
            };
        }
        else if (exception is GatewayApi.ApiException gatewayApiException)
        {
            // GatewayApi.ApiException is returned if we get a 500 from upstream
            _logger.Log(LogLevel.Information, exception, "Error response from upstream server");
            var (exceptionName, causeName) = ExtractExceptionAndCause(gatewayApiException.ErrorContent.ToString() ?? string.Empty);
            outException = new HttpResponseException(causeName)
            {
                Status = gatewayApiException.ErrorCode,
                ExceptionNameUpperSnakeCase = exceptionName ?? "UNKNOWN_ERROR",
            };
        }
        else
        {
            _logger.Log(LogLevel.Warning, exception, "Unknown exception");
            outException = new HttpResponseException(); // Hide error codes behind a blanket UNKNOWN_ERROR
        }

        context.Result = new ObjectResult(new
        {
            exception = outException.ExceptionNameUpperSnakeCase,
            cause = outException.Cause,
        })
        {
            StatusCode = outException.Status,
        };
        context.ExceptionHandled = true;
    }

    private static (string? Exception, string? Cause) ExtractExceptionAndCause(string upstreamErrorResponse)
    {
        try
        {
            using var jsonDocument = JsonDocument.Parse(upstreamErrorResponse);
            var details = jsonDocument.RootElement.GetProperty("details");

            details.TryGetProperty("cause", out var causeProperty);
            details.TryGetProperty("exception", out var exceptionProperty);
            return (exceptionProperty.GetString(), causeProperty.GetString());
        }
        catch (Exception)
        {
            return (null, null);
        }
    }
}
