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

using Microsoft.Extensions.Logging;
using RadixDlt.NetworkGateway.Abstractions;
using RadixDlt.NetworkGateway.Abstractions.Exceptions;
using RadixDlt.NetworkGateway.Abstractions.Extensions;
using RadixDlt.NetworkGateway.GatewayApi.CoreCommunications;
using RadixDlt.NetworkGateway.GatewayApi.Exceptions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CoreModel = RadixDlt.CoreApiSdk.Model;
using GatewayModel = RadixDlt.NetworkGateway.GatewayApiSdk.Model;
using ToolkitModel = RadixDlt.RadixEngineToolkit.Model;

namespace RadixDlt.NetworkGateway.GatewayApi.Services;

public interface ISubmissionService
{
    Task<GatewayModel.TransactionSubmitResponse> HandleSubmitRequest(GatewayModel.TransactionSubmitRequest request, CancellationToken token = default);
}

internal class SubmissionService : ISubmissionService
{
    private readonly ICoreApiHandler _coreApiHandler;
    private readonly ISubmissionTrackingService _submissionTrackingService;
    private readonly IClock _clock;
    private readonly IEnumerable<ISubmissionServiceObserver> _observers;
    private readonly ILogger _logger;

    public SubmissionService(
        ICoreApiHandler coreApiHandler,
        ISubmissionTrackingService submissionTrackingService,
        IClock clock,
        IEnumerable<ISubmissionServiceObserver> observers,
        ILogger<SubmissionService> logger)
    {
        _coreApiHandler = coreApiHandler;
        _submissionTrackingService = submissionTrackingService;
        _clock = clock;
        _observers = observers;
        _logger = logger;
    }

    public async Task<GatewayModel.TransactionSubmitResponse> HandleSubmitRequest(GatewayModel.TransactionSubmitRequest request, CancellationToken token = default)
    {
        var parsedTransaction = await HandlePreSubmissionParseTransaction(request);
        var submittedTimestamp = _clock.UtcNow;

        var trackingGuidance = await _submissionTrackingService.TrackInitialSubmission(
            submittedTimestamp,
            parsedTransaction,
            _coreApiHandler.GetCoreNodeConnectedTo().Name,
            token
        );

        if (trackingGuidance.FailureReason != null)
        {
            await _observers.ForEachAsync(x => x.SubmissionAlreadyFailed(request, trackingGuidance));

            throw InvalidTransactionException.FromPreviouslyFailedTransactionError(trackingGuidance.FailureReason);
        }

        if (!trackingGuidance.ShouldSubmitToNode)
        {
            await _observers.ForEachAsync(x => x.SubmissionAlreadySubmitted(request, trackingGuidance));

            return new GatewayModel.TransactionSubmitResponse(
                duplicate: true
            );
        }

        try
        {
            await _observers.ForEachAsync(x => x.PreHandleSubmitRequest(request));

            var response = await HandleSubmitAndCreateResponse(request, parsedTransaction, token);

            await _observers.ForEachAsync(x => x.PostHandleSubmitRequest(request, response));

            return response;
        }
        catch (Exception ex)
        {
            await _observers.ForEachAsync(x => x.HandleSubmitRequestFailed(request, ex));

            throw;
        }
    }

    private async Task<ToolkitModel.Transaction.NotarizedTransaction> HandlePreSubmissionParseTransaction(GatewayModel.TransactionSubmitRequest request)
    {
        try
        {
            var notarizedTransaction = ToolkitModel.Transaction.NotarizedTransaction.Decompile(request.GetNotarizedTransactionBytes());
            var validationConfig = new ToolkitModel.Exchange.ValidationConfig(_coreApiHandler.GetNetworkId());
            var staticValidationResult = notarizedTransaction.StaticallyValidate(validationConfig);

            if (staticValidationResult is ToolkitModel.Exchange.StaticallyValidateTransactionResponse.Invalid invalid)
            {
                await _observers.ForEachAsync(x => x.ParsedTransactionStaticallyInvalid(request, invalid.Error));

                throw InvalidTransactionException.FromStaticallyInvalid(staticValidationResult.ToString());
            }

            return notarizedTransaction;
        }
        catch (RadixEngineToolkit.Exceptions.EngineToolkitRequestError ex)
        {
            await _observers.ForEachAsync(x => x.ParsedTransactionUnsupportedPayloadType(request, ex));

            throw InvalidTransactionException.FromUnsupportedPayloadType();
        }
        catch (Exception ex)
        {
            await _observers.ForEachAsync(x => x.ParseTransactionFailedUnknown(request, ex));

            throw;
        }
    }

    private async Task<GatewayModel.TransactionSubmitResponse> HandleSubmitAndCreateResponse(
        GatewayModel.TransactionSubmitRequest request,
        ToolkitModel.Transaction.NotarizedTransaction parsedTransaction,
        CancellationToken token)
    {
        using var timeoutTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(3)); // TODO configurable
        using var finalTokenSource = CancellationTokenSource.CreateLinkedTokenSource(timeoutTokenSource.Token, token);

        try
        {
            var result = await _coreApiHandler.SubmitTransaction(
                new CoreModel.TransactionSubmitRequest(
                    _coreApiHandler.GetNetworkName(),
                    request.NotarizedTransactionHex
                ),
                finalTokenSource.Token
            );

            if (result.Duplicate)
            {
                await _observers.ForEachAsync(x => x.SubmissionDuplicate(request, result));
            }
            else
            {
                await _observers.ForEachAsync(x => x.SubmissionSucceeded(request, result));
            }

            return new GatewayModel.TransactionSubmitResponse(
                duplicate: result.Duplicate
            );
        }
        catch (WrappedCoreApiException ex) when (ex.Properties.Transience == CoreApiErrorTransience.Permanent)
        {
            // Any other known Core exception which can't result in the transaction being submitted
            await _observers.ForEachAsync(x => x.HandleSubmissionFailedPermanently(request, ex));

            await _submissionTrackingService.MarkAsFailed(
                true,
                parsedTransaction.TransactionHash(),
                ex.Error.Message,
                token
            );

            throw InvalidTransactionException.FromInvalidTransactionDueToCoreApiException(ex);
        }
        catch (WrappedCoreApiException ex) when (ex.Properties.Transience == CoreApiErrorTransience.Transient)
        {
            // Any other known Core exception which can't result in the transaction being submitted
            await _observers.ForEachAsync(x => x.HandleSubmissionFailedTemporary(request, ex));

            await _submissionTrackingService.MarkAsFailed(
                false,
                parsedTransaction.TransactionHash(),
                ex.Error.Message,
                token
            );
        }
        catch (OperationCanceledException ex) when (timeoutTokenSource.Token.IsCancellationRequested)
        {
            await _observers.ForEachAsync(x => x.HandleSubmissionFailedTimeout(request, ex));

            _logger.LogWarning(ex, "Request timeout submitting transaction with hash {TransactionHash}", request.NotarizedTransactionHex);
        }
        catch (Exception ex)
        {
            // Any other kind of exception is unknown - eg it a connection drop or a 500 from the Core API.
            // In theory, the transaction could have been submitted -- so we return success and
            // if it wasn't submitted successfully, it'll be retried automatically by the resubmission service in
            // any case.
            await _observers.ForEachAsync(x => x.HandleSubmissionFailedUnknown(request, ex));

            _logger.LogWarning(ex, "Unknown error submitting transaction with hash {TransactionHash}", request.NotarizedTransactionHex);
        }

        return new GatewayModel.TransactionSubmitResponse(
            duplicate: false
        );
    }
}
