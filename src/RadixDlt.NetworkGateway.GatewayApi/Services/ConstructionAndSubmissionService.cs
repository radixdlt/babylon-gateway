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
using Prometheus;
using RadixCoreApi.Generated.Model;
using RadixDlt.NetworkGateway.Core.Database.Models.Mempool;
using RadixDlt.NetworkGateway.Core.Exceptions;
using RadixDlt.NetworkGateway.Core.Extensions;
using RadixDlt.NetworkGateway.Core.StaticHelpers;
using RadixDlt.NetworkGateway.GatewayApi.CoreCommunications;
using RadixDlt.NetworkGateway.GatewayApi.Exceptions;
using CoreModel = RadixCoreApi.Generated.Model;
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

    /* Metrics */
    private static readonly Counter _transactionBuildRequestCount = Metrics
        .CreateCounter(
            "ng_construction_transaction_build_request_count",
            "Number of transaction build requests"
        );

    private static readonly Counter _transactionBuildSuccessCount = Metrics
        .CreateCounter(
            "ng_construction_transaction_build_success_count",
            "Number of transaction build successes"
        );

    private static readonly Counter _transactionBuildErrorCount = Metrics
        .CreateCounter(
            "ng_construction_transaction_build_error_count",
            "Number of transaction build errors"
        );

    private static readonly Counter _transactionFinalizeRequestCount = Metrics
        .CreateCounter(
            "ng_construction_transaction_finalize_request_count",
            "Number of transaction finalize requests"
        );

    private static readonly Counter _transactionFinalizeSuccessCount = Metrics
        .CreateCounter(
            "ng_construction_transaction_finalize_success_count",
            "Number of transaction finalize successes"
        );

    private static readonly Counter _transactionFinalizeErrorCount = Metrics
        .CreateCounter(
            "ng_construction_transaction_finalize_error_count",
            "Number of transaction finalize errors"
        );

    private static readonly Counter _transactionSubmitRequestCount = Metrics
        .CreateCounter(
            "ng_construction_transaction_submission_request_count",
            "Number of transaction submission requests (including as part of a finalize request)"
        );

    private static readonly Counter _transactionSubmitSuccessCount = Metrics
        .CreateCounter(
            "ng_construction_transaction_submission_success_count",
            "Number of transaction submission successes (including as part of a finalize request)"
        );

    private static readonly Counter _transactionSubmitErrorCount = Metrics
        .CreateCounter(
            "ng_construction_transaction_submission_error_count",
            "Number of transaction submission errors (including as part of a finalize request)"
        );

    private static readonly Counter _transactionSubmitResolutionByResultCount = Metrics
        .CreateCounter(
            "ng_construction_transaction_submission_resolution_count",
            "Number of various resolutions at transaction submission time",
            new CounterConfiguration { LabelNames = new[] { "result" } }
        );

    /* Dependencies */
    private readonly IValidations _validations;
    private readonly ICoreApiHandler _coreApiHandler;
    private readonly INetworkConfigurationProvider _networkConfigurationProvider;
    private readonly ISubmissionTrackingService _submissionTrackingService;
    private readonly ILogger<ConstructionAndSubmissionService> _logger;

    public ConstructionAndSubmissionService(
        IValidations validations,
        ICoreApiHandler coreApiHandler,
        INetworkConfigurationProvider networkConfigurationProvider,
        ISubmissionTrackingService submissionTrackingService,
        ILogger<ConstructionAndSubmissionService> logger
    )
    {
        _validations = validations;
        _coreApiHandler = coreApiHandler;
        _networkConfigurationProvider = networkConfigurationProvider;
        _submissionTrackingService = submissionTrackingService;
        _logger = logger;
    }

    public async Task<Gateway.TransactionBuild> HandleBuildRequest(Gateway.TransactionBuildRequest request, Gateway.LedgerState ledgerState)
    {
        _transactionBuildRequestCount.Inc();
        try
        {
            var response = await HandleBuildAndCreateResponse(request, ledgerState);
            _transactionBuildSuccessCount.Inc();
            return response;
        }
        catch (Exception)
        {
            _transactionBuildErrorCount.Inc();
            throw;
        }
    }

    public async Task<Gateway.TransactionFinalizeResponse> HandleFinalizeRequest(
        Gateway.TransactionFinalizeRequest request
    )
    {
        _transactionFinalizeRequestCount.Inc();
        try
        {
            var response = await HandleFinalizeAndCreateResponse(request);
            _transactionFinalizeSuccessCount.Inc();
            return response;
        }
        catch (Exception)
        {
            _transactionFinalizeErrorCount.Inc();
            throw;
        }
    }

    public async Task<Gateway.TransactionSubmitResponse> HandleSubmitRequest(
        Gateway.TransactionSubmitRequest request
    )
    {
        _transactionSubmitRequestCount.Inc();
        try
        {
            var response = await HandleSubmitAndCreateResponse(request);
            _transactionSubmitSuccessCount.Inc();
            return response;
        }
        catch (Exception)
        {
            _transactionSubmitErrorCount.Inc();
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
            _transactionSubmitResolutionByResultCount.WithLabels("parse_failed_substate_missing_or_already_used").Inc();
            throw InvalidTransactionException.FromSubstateDependencyNotFoundError(signedTransaction.AsString, ex.Error);
        }
        catch (WrappedCoreApiException ex) when (ex.Properties.MarksInvalidTransaction)
        {
            _transactionSubmitResolutionByResultCount.WithLabels("parse_failed_invalid_transaction").Inc();
            throw InvalidTransactionException.FromInvalidTransactionDueToCoreApiException(signedTransaction.AsString, ex);
        }
        catch (Exception)
        {
            _transactionSubmitResolutionByResultCount.WithLabels("parse_failed_unknown_error").Inc();
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
            _transactionSubmitResolutionByResultCount.WithLabels("already_failed").Inc();
            throw InvalidTransactionException.FromPreviouslyFailedTransactionError(
                signedTransaction.AsString,
                mempoolTrackGuidance.TransactionAlreadyFailedReason.Value
            );
        }

        if (!mempoolTrackGuidance.ShouldSubmitToNode)
        {
            _transactionSubmitResolutionByResultCount.WithLabels("already_submitted").Inc();
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
                _transactionSubmitResolutionByResultCount.WithLabels("node_marks_as_duplicate").Inc();
            }
            else
            {
                _transactionSubmitResolutionByResultCount.WithLabels("success").Inc();
            }
        }
        catch (WrappedCoreApiException<SubstateDependencyNotFoundError> ex)
        {
            _transactionSubmitResolutionByResultCount.WithLabels("substate_missing_or_already_used").Inc();
            await _submissionTrackingService.MarkAsFailed(
                transactionIdentifierHash,
                MempoolTransactionFailureReason.DoubleSpend,
                "A substate identifier the transaction uses is missing or already downed"
            );
            throw InvalidTransactionException.FromSubstateDependencyNotFoundError(signedTransaction.AsString, ex.Error);
        }
        catch (WrappedCoreApiException ex) when (ex.Properties.MarksInvalidTransaction)
        {
            _transactionSubmitResolutionByResultCount.WithLabels("invalid_transaction").Inc();
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
            _transactionSubmitResolutionByResultCount.WithLabels("unknown_permanent_error").Inc();
            await _submissionTrackingService.MarkAsFailed(
                transactionIdentifierHash,
                MempoolTransactionFailureReason.Unknown,
                $"Core API Exception: {ex.Error.GetType().Name} without undefined behaviour on initial submission"
            );
            throw;
        }
        catch (OperationCanceledException ex)
        {
            _transactionSubmitResolutionByResultCount.WithLabels("request_timeout").Inc();
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
            _transactionSubmitResolutionByResultCount.WithLabels("unknown_error").Inc();
            _logger.LogWarning(
                ex,
                "Unknown error submitting transaction with hash {TransactionHash}",
                transactionIdentifierHash.ToHex()
            );
        }
    }
}
