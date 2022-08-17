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
using NodaTime;
using RadixDlt.CoreApiSdk.Model;
using RadixDlt.NetworkGateway.Common.Exceptions;
using RadixDlt.NetworkGateway.Common.Extensions;
using RadixDlt.NetworkGateway.Common.Model;
using RadixDlt.NetworkGateway.Common.StaticHelpers;
using RadixDlt.NetworkGateway.GatewayApi.CoreCommunications;
using RadixDlt.NetworkGateway.GatewayApi.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;
using CoreModel = RadixDlt.CoreApiSdk.Model;
using Gateway = RadixDlt.NetworkGateway.GatewayApiSdk.Model;

namespace RadixDlt.NetworkGateway.GatewayApi.Services;

public interface IConstructionAndSubmissionService
{
    Task<Gateway.TransactionBuild> HandleBuildRequest(Gateway.TransactionBuildRequest request, Gateway.LedgerState ledgerState);

    Task<Gateway.TransactionFinalizeResponse> HandleFinalizeRequest(Gateway.TransactionFinalizeRequest request);

    Task<Gateway.TransactionSubmitResponse> HandleSubmitRequest(Gateway.TransactionSubmitRequest request);
}

public class ConstructionAndSubmissionService : IConstructionAndSubmissionService
{
    private static readonly int MaximumMessageLengthInBytes = 255;

    /* Dependencies */
    private readonly IValidations _validations;
    private readonly ICoreApiHandler _coreApiHandler;
    private readonly INetworkConfigurationProvider _networkConfigurationProvider;
    private readonly ISubmissionTrackingService _submissionTrackingService;
    private readonly ILogger<ConstructionAndSubmissionService> _logger;
    private readonly IConstructionAndSubmissionServiceObserver? _observer;

    public ConstructionAndSubmissionService(
        IValidations validations,
        ICoreApiHandler coreApiHandler,
        INetworkConfigurationProvider networkConfigurationProvider,
        ISubmissionTrackingService submissionTrackingService,
        ILogger<ConstructionAndSubmissionService> logger,
        IConstructionAndSubmissionServiceObserver? observer = null)
    {
        _validations = validations;
        _coreApiHandler = coreApiHandler;
        _networkConfigurationProvider = networkConfigurationProvider;
        _submissionTrackingService = submissionTrackingService;
        _logger = logger;
        _observer = observer;
    }

    public async Task<Gateway.TransactionBuild> HandleBuildRequest(Gateway.TransactionBuildRequest request, Gateway.LedgerState ledgerState)
    {
        if (_observer != null)
        {
            await _observer.PreHandleBuildRequest(request, ledgerState);
        }

        try
        {
            var response = await HandleBuildAndCreateResponse(request, ledgerState);

            if (_observer != null)
            {
                await _observer.PostHandleBuildRequest(request, ledgerState, response);
            }

            return response;
        }
        catch (Exception ex)
        {
            if (_observer != null)
            {
                await _observer.HandleBuildRequestFailed(request, ledgerState, ex);
            }

            throw;
        }
    }

    public async Task<Gateway.TransactionFinalizeResponse> HandleFinalizeRequest(Gateway.TransactionFinalizeRequest request)
    {
        if (_observer != null)
        {
            await _observer.PreHandleFinalizeRequest(request);
        }

        try
        {
            var response = await HandleFinalizeAndCreateResponse(request);

            if (_observer != null)
            {
                await _observer.PostHandleFinalizeRequest(request, response);
            }

            return response;
        }
        catch (Exception ex)
        {
            if (_observer != null)
            {
                await _observer.HandleFinalizeRequestFailed(request, ex);
            }

            throw;
        }
    }

    public async Task<Gateway.TransactionSubmitResponse> HandleSubmitRequest(Gateway.TransactionSubmitRequest request)
    {
        if (_observer != null)
        {
            await _observer.PreHandleSubmitRequest(request);
        }

        try
        {
            var response = await HandleSubmitAndCreateResponse(request);

            if (_observer != null)
            {
                await _observer.PostHandleSubmitRequest(request, response);
            }

            return response;
        }
        catch (Exception ex)
        {
            if (_observer != null)
            {
                await _observer.HandleSubmitRequestFailed(request, ex);
            }

            throw;
        }
    }

    private async Task<Gateway.TransactionBuild> HandleBuildAndCreateResponse(Gateway.TransactionBuildRequest request, Gateway.LedgerState ledgerState)
    {
        var coreBuildResponse = await BuildTransaction(request, ledgerState);

        var coreParseResponse = await _coreApiHandler.ParseTransaction(new CoreModel.ConstructionParseRequest(
            networkIdentifier: _coreApiHandler.GetNetworkIdentifier(),
            transaction: coreBuildResponse.UnsignedTransaction,
            signed: false
        ));

        var unsignedTransactionPayload = StringExtensions.ConvertFromHex(coreBuildResponse.UnsignedTransaction);
        var payloadToSign = StringExtensions.ConvertFromHex(coreBuildResponse.PayloadToSign);

        if (!RadixHashing.IsValidPayloadToSign(unsignedTransactionPayload, payloadToSign))
        {
            throw new InvalidCoreApiResponseException(
                $"Built transaction was claimed to have payload to sign {payloadToSign.ToHex()} " +
                $"but the Gateway calculates it as {RadixHashing.CreatePayloadToSignFromUnsignedTransactionPayload(unsignedTransactionPayload)} " +
                $"(transaction contents: {unsignedTransactionPayload.ToHex()}"
            );
        }

        return new Gateway.TransactionBuild(
            fee: coreParseResponse.Metadata.Fee.AsGatewayTokenAmount(),
            unsignedTransaction: unsignedTransactionPayload.ToHex(),
            payloadToSign: payloadToSign.ToHex()
        );
    }

    private async Task<Gateway.TransactionFinalizeResponse> HandleFinalizeAndCreateResponse(Gateway.TransactionFinalizeRequest request)
    {
        var coreFinalizeResponse = await HandleCoreFinalizeRequest(request, new CoreModel.ConstructionFinalizeRequest(
            _coreApiHandler.GetNetworkIdentifier(),
            unsignedTransaction: _validations.ExtractValidHex("Unsigned transaction", request.UnsignedTransaction).AsString,
            signature: new CoreModel.Signature(
                publicKey: new CoreModel.PublicKey(
                    _validations.ExtractValidPublicKey(request.Signature.PublicKey).AsString
                ),
                bytes: _validations.ExtractValidHex("Signature Bytes", request.Signature.Bytes).AsString
            )
        ));

        var transactionHashIdentifier = RadixHashing.CreateTransactionHashIdentifierFromSignTransactionPayload(
            StringExtensions.ConvertFromHex(coreFinalizeResponse.SignedTransaction)
        );

        if (request.Submit)
        {
            await HandleSubmitRequest(
                new Gateway.TransactionSubmitRequest(
                    signedTransaction: coreFinalizeResponse.SignedTransaction
                )
            );
        }

        return new Gateway.TransactionFinalizeResponse(
            signedTransaction: coreFinalizeResponse.SignedTransaction,
            transactionIdentifier: transactionHashIdentifier.AsGatewayTransactionIdentifier()
        );
    }

    private async Task<Gateway.TransactionSubmitResponse> HandleSubmitAndCreateResponse(Gateway.TransactionSubmitRequest request)
    {
        var signedTransactionContents = _validations.ExtractValidHex("Signed transaction", request.SignedTransaction);
        var transactionHashIdentifier = RadixHashing.CreateTransactionHashIdentifierFromSignTransactionPayload(
            signedTransactionContents.Bytes
        );

        await HandleSubmission(signedTransactionContents, transactionHashIdentifier);

        return new Gateway.TransactionSubmitResponse(
            transactionIdentifier: transactionHashIdentifier.AsGatewayTransactionIdentifier()
        );
    }

    private async Task<CoreModel.ConstructionBuildResponse> BuildTransaction(Gateway.TransactionBuildRequest request, Gateway.LedgerState ledgerState)
    {
        var feePayer = _validations.ExtractValidAccountAddress(request.FeePayer);
        var validatedMessage = _validations.ExtractOptionalValidHexOrNull("Message", request.Message);

        if (validatedMessage != null && validatedMessage.Bytes.Length > MaximumMessageLengthInBytes)
        {
            throw new MessageTooLongException(MaximumMessageLengthInBytes, validatedMessage.Bytes.Length);
        }

        var transactionBuilder = new TransactionBuilder(
            _validations,
            _networkConfigurationProvider,
            ledgerState,
            feePayer
        );

        // This performs checks against known ledger, and throws relevant exceptions, eg if a user doesn't have enough
        // funds for a given action. However -- it doesn't know how much fees will be at this point.
        var mappedTransaction = await transactionBuilder.MapAndValidateActions(request.Actions);

        return new CoreModel.ConstructionBuildResponse(); // TODO - Work out what to do to support legacy build
    }

    private async Task<CoreModel.ConstructionFinalizeResponse> HandleCoreFinalizeRequest(
        Gateway.TransactionFinalizeRequest gatewayRequest,
        CoreModel.ConstructionFinalizeRequest request
    )
    {
        try
        {
            return await _coreApiHandler.FinalizeTransaction(request);
        }
        catch (WrappedCoreApiException<CoreModel.InvalidSignatureError>)
        {
            throw new InvalidSignatureException(gatewayRequest.Signature);
        }
    }

    private async Task<CoreModel.ConstructionParseResponse> HandlePreSubmissionParseSignedTransaction(
        ValidatedHex signedTransaction
    )
    {
        try
        {
            return await _coreApiHandler.ParseTransaction(new CoreModel.ConstructionParseRequest(
                networkIdentifier: _coreApiHandler.GetNetworkIdentifier(),
                transaction: signedTransaction.AsString,
                signed: true
            ));
        }
        catch (WrappedCoreApiException<SubstateDependencyNotFoundError> ex)
        {
            if (_observer != null)
            {
                await _observer.ParseTransactionFailedSubstateNotFound(signedTransaction, ex);
            }

            throw InvalidTransactionException.FromSubstateDependencyNotFoundError(signedTransaction.AsString, ex.Error);
        }
        catch (WrappedCoreApiException ex) when (ex.Properties.MarksInvalidTransaction)
        {
            if (_observer != null)
            {
                await _observer.ParseTransactionFailedInvalidTransaction(signedTransaction, ex);
            }

            throw InvalidTransactionException.FromInvalidTransactionDueToCoreApiException(signedTransaction.AsString, ex);
        }
        catch (Exception ex)
        {
            if (_observer != null)
            {
                await _observer.ParseTransactionFailedUnknown(signedTransaction, ex);
            }

            throw;
        }
    }

    // NB - The error handling here should mirror the resubmission in MempoolResubmissionService
    private async Task HandleSubmission(
        ValidatedHex signedTransaction,
        byte[] transactionIdentifierHash
    )
    {
        var parseResponse = await HandlePreSubmissionParseSignedTransaction(signedTransaction);

        var submittedTimestamp = SystemClock.Instance.GetCurrentInstant();
        using var submissionTimeoutCts = new CancellationTokenSource(TimeSpan.FromMilliseconds(3000));

        var mempoolTrackGuidance = await _submissionTrackingService.TrackInitialSubmission(
            submittedTimestamp,
            signedTransaction.Bytes,
            transactionIdentifierHash,
            _coreApiHandler.GetCoreNodeConnectedTo().Name,
            parseResponse
        );

        if (mempoolTrackGuidance.TransactionAlreadyFailedReason != null)
        {
            if (_observer != null)
            {
                await _observer.SubmissionAlreadyFailed(signedTransaction, mempoolTrackGuidance);
            }

            throw InvalidTransactionException.FromPreviouslyFailedTransactionError(
                signedTransaction.AsString,
                mempoolTrackGuidance.TransactionAlreadyFailedReason.Value
            );
        }

        if (!mempoolTrackGuidance.ShouldSubmitToNode)
        {
            if (_observer != null)
            {
                await _observer.SubmissionAlreadySubmitted(signedTransaction, mempoolTrackGuidance);
            }

            return;
        }

        try
        {
            var result = await _coreApiHandler.SubmitTransaction(
                new CoreModel.ConstructionSubmitRequest(
                    _coreApiHandler.GetNetworkIdentifier(),
                    signedTransaction.AsString
                ),
                submissionTimeoutCts.Token
            );

            if (result.Duplicate)
            {
                if (_observer != null)
                {
                    await _observer.SubmissionDuplicate(signedTransaction, result);
                }
            }
            else
            {
                if (_observer != null)
                {
                    await _observer.SubmissionSucceeded(signedTransaction, result);
                }
            }
        }
        catch (WrappedCoreApiException<SubstateDependencyNotFoundError> ex)
        {
            if (_observer != null)
            {
                await _observer.HandleSubmissionFailedSubstateNotFound(signedTransaction, ex);
            }

            await _submissionTrackingService.MarkAsFailed(
                transactionIdentifierHash,
                MempoolTransactionFailureReason.DoubleSpend,
                "A substate identifier the transaction uses is missing or already downed"
            );

            throw InvalidTransactionException.FromSubstateDependencyNotFoundError(signedTransaction.AsString, ex.Error);
        }
        catch (WrappedCoreApiException ex) when (ex.Properties.MarksInvalidTransaction)
        {
            if (_observer != null)
            {
                await _observer.HandleSubmissionFailedInvalidTransaction(signedTransaction, ex);
            }

            await _submissionTrackingService.MarkAsFailed(
                transactionIdentifierHash,
                MempoolTransactionFailureReason.Unknown,
                $"Core API Exception: {ex.Error.GetType().Name} marking invalid transaction on initial submission"
            );

            throw InvalidTransactionException.FromInvalidTransactionDueToCoreApiException(signedTransaction.AsString, ex);
        }
        catch (WrappedCoreApiException ex) when (ex.Properties.Transience == Transience.Permanent)
        {
            // Any other known Core exception which can't result in the transaction being submitted

            if (_observer != null)
            {
                await _observer.HandleSubmissionFailedPermanently(signedTransaction, ex);
            }

            await _submissionTrackingService.MarkAsFailed(
                transactionIdentifierHash,
                MempoolTransactionFailureReason.Unknown,
                $"Core API Exception: {ex.Error.GetType().Name} without undefined behaviour on initial submission"
            );

            throw;
        }
        catch (OperationCanceledException ex)
        {
            if (_observer != null)
            {
                await _observer.HandleSubmissionFailedTimeout(signedTransaction, ex);
            }

            _logger.LogWarning(
                ex,
                "Request timeout submitting transaction with hash {TransactionHash}",
                transactionIdentifierHash.ToHex()
            );
        }
        catch (Exception ex)
        {
            // Any other kind of exception is unknown - eg it a connection drop or a 500 from the Core API.
            // In theory, the transaction could have been submitted -- so we return success and
            // if it wasn't submitted successfully, it'll be retried automatically by the resubmission service in
            // any case.

            if (_observer != null)
            {
                await _observer.HandleSubmissionFailedUnknown(signedTransaction, ex);
            }

            _logger.LogWarning(
                ex,
                "Unknown error submitting transaction with hash {TransactionHash}",
                transactionIdentifierHash.ToHex()
            );
        }
    }
}
