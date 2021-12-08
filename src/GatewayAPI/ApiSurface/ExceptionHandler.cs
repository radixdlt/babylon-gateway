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
using Newtonsoft.Json;
using RadixGatewayApi.Generated.Model;
using CoreApi = RadixCoreApi.Generated.Client;
using GatewayApi = RadixGatewayApi.Generated.Client;

namespace GatewayAPI.ApiSurface;

public interface IExceptionHandler
{
    ActionResult CreateAndLogApiResultFromException(Exception exception, string traceId);
}

public class ExceptionHandler : IExceptionHandler
{
    private readonly ILogger<ExceptionHandler> _logger;
    private LogLevel _knownErrorLogLevel;

    public ExceptionHandler(ILogger<ExceptionHandler> logger, IHostEnvironment env)
    {
        _logger = logger;
        _knownErrorLogLevel = env.IsDevelopment() ? LogLevel.Information : LogLevel.Debug;
    }

    public ActionResult CreateAndLogApiResultFromException(Exception exception, string traceId)
    {
        KnownGatewayErrorException gatewayErrorException;

        if (exception is KnownGatewayErrorException httpResponseException)
        {
            _logger.Log(_knownErrorLogLevel, exception, "Known exception with http response code [RequestTrace={TraceId}]", traceId);
            gatewayErrorException = httpResponseException;
        }
        else if (exception is HttpRequestException)
        {
            // HttpRequestException is returned from the Gateway or Core APIs if we can't connect
            _logger.Log(LogLevel.Information, exception, "Error relaying request to upstream server [RequestTrace={TraceId}]", traceId);
            gatewayErrorException = InternalServerException.OfInvalidGatewayException(traceId);
        }
        else if (exception is CoreApi.ApiException coreApiException)
        {
            // CoreApi.ApiException is returned if we get a 500 from upstream
            _logger.Log(LogLevel.Information, exception, "Unhandled error response from upstream core API [RequestTrace={TraceId}]", traceId);
            gatewayErrorException = InternalServerException.OfUnhandledCoreApiException(coreApiException.ErrorContent.ToString() ?? string.Empty, traceId);
        }
        else if (exception is WrappedCoreApiException wrappedCoreApiException)
        {
            // CoreApi.ApiException is returned if we get a 500 from upstream
            _logger.Log(LogLevel.Information, exception, "Unhandled error response from upstream core API [RequestTrace={TraceId}]", traceId);
            gatewayErrorException = InternalServerException.OfUnhandledCoreApiException(wrappedCoreApiException.ApiException.ErrorContent.ToString() ?? string.Empty, traceId);
        }
        else if (exception is GatewayApi.ApiException gatewayApiException)
        {
            // GatewayApi.ApiException is returned if we get a 500 from upstream
            var upstreamError = ExtractUpstreamGatewayErrorResponse(gatewayApiException.ErrorContent.ToString() ?? string.Empty);
            if (upstreamError != null)
            {
                _logger.Log(LogLevel.Information, exception, "Error response from upstream gateway API [RequestTrace={TraceId}]", traceId);
                gatewayErrorException = UpstreamGatewayApiException.OfUpstreamGatewayApiError(upstreamError);
            }
            else
            {
                _logger.Log(LogLevel.Warning, exception, "Error response from upstream gateway API with unparsable error response [RequestTrace={TraceId}]", traceId);
                gatewayErrorException = InternalServerException.OfHiddenException(exception, traceId);
            }
        }
        else if (exception is InvalidCoreApiResponseException invalidCoreApiResponseException)
        {
            _logger.Log(LogLevel.Warning, exception, "Invalid Core API response [RequestTrace={TraceId}]", traceId);
            gatewayErrorException = InternalServerException.OfInvalidCoreApiResponseException(invalidCoreApiResponseException, traceId);
        }
        else
        {
            _logger.Log(LogLevel.Warning, exception, "Unknown exception [RequestTrace={TraceId}]", traceId);
            gatewayErrorException = InternalServerException.OfHiddenException(exception, traceId);
        }

        return new JsonResult(new ErrorResponse(
            code: gatewayErrorException.StatusCode,
            message: gatewayErrorException.UserFacingMessage,
            details: gatewayErrorException.GatewayError,
            traceId: traceId
        ))
        {
            StatusCode = 500,
        };
    }

    private static ErrorResponse? ExtractUpstreamGatewayErrorResponse(string upstreamErrorResponse)
    {
        try
        {
            return JsonConvert.DeserializeObject<ErrorResponse>(upstreamErrorResponse);
        }
        catch (Exception)
        {
            return null;
        }
    }
}
