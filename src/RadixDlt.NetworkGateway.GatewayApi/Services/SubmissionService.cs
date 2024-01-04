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
using Microsoft.Extensions.Options;
using RadixDlt.NetworkGateway.Abstractions.Configuration;
using RadixDlt.NetworkGateway.Abstractions.Extensions;
using RadixDlt.NetworkGateway.Abstractions.Network;
using RadixDlt.NetworkGateway.GatewayApi.Configuration;
using RadixDlt.NetworkGateway.GatewayApi.CoreCommunications;
using RadixDlt.NetworkGateway.GatewayApi.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GatewayModel = RadixDlt.NetworkGateway.GatewayApiSdk.Model;
using ToolkitModel = RadixEngineToolkit;

namespace RadixDlt.NetworkGateway.GatewayApi.Services;

public interface ISubmissionService
{
    Task<GatewayModel.TransactionSubmitResponse> HandleSubmitRequest(GatewayModel.LedgerState ledgerState, GatewayModel.TransactionSubmitRequest request, CancellationToken token = default);
}

internal class SubmissionService : ISubmissionService
{
    private readonly INetworkConfigurationProvider _networkConfigurationProvider;
    private readonly ICoreApiHandler _coreApiHandler;
    private readonly ISubmissionTrackingService _submissionTrackingService;
    private readonly IReadOnlyCollection<ISubmissionServiceObserver> _observers;
    private readonly IOptionsMonitor<CoreApiIntegrationOptions> _coreApiIntegrationOptions;
    private readonly ILogger<SubmissionService> _logger;

    public SubmissionService(
        INetworkConfigurationProvider networkConfigurationProvider,
        ICoreApiHandler coreApiHandler,
        ISubmissionTrackingService submissionTrackingService,
        IEnumerable<ISubmissionServiceObserver> observers,
        IOptionsMonitor<CoreApiIntegrationOptions> coreApiIntegrationOptions,
        ILogger<SubmissionService> logger)
    {
        _networkConfigurationProvider = networkConfigurationProvider;
        _coreApiHandler = coreApiHandler;
        _submissionTrackingService = submissionTrackingService;
        _observers = observers.ToArray();
        _coreApiIntegrationOptions = coreApiIntegrationOptions;
        _logger = logger;
    }

    public async Task<GatewayModel.TransactionSubmitResponse> HandleSubmitRequest(GatewayModel.LedgerState ledgerState, GatewayModel.TransactionSubmitRequest request, CancellationToken token = default)
    {
        var transactionBytes = request.GetNotarizedTransactionBytes();
        // If these checks fails, it's not worth saving this transaction to our database or submitting it to the node.
        using var parsedTransaction = await HandlePreSubmissionParseTransaction(transactionBytes);
        await CheckPendingTransactionEpochValidity(ledgerState, parsedTransaction);

        var targetNode = _coreApiHandler.GetCoreNodeConnectedTo();
        var options = _coreApiIntegrationOptions.CurrentValue;
        var submissionResult = await _submissionTrackingService.ObserveSubmissionToGatewayAndSubmitToNetworkIfNew(
            _coreApiHandler.GetTransactionApi(),
            (await _networkConfigurationProvider.GetNetworkConfiguration(token)).Name,
            targetNode.Name,
            new PendingTransactionHandlingConfig(
                options.MaxSubmissionAttempts,
                options.StopResubmittingAfter,
                options.BaseDelayBetweenResubmissions,
                options.ResubmissionDelayBackoffExponent),
            parsedTransaction,
            transactionBytes,
            _coreApiIntegrationOptions.CurrentValue.SubmitTransactionTimeout,
            ledgerState.Epoch,
            token
        );

        // Note: I'm not sure this is the correct choice of what should be an API error here, but I don't want to change the API errors that are thrown this close to launch
        if (submissionResult.PermanentlyRejectedReason != null)
        {
            await _observers.ForEachAsync(x => x.ObserveTransactionSubmissionToGatewayOutcome(TransactionSubmissionOutcome.PermanentlyRejected, targetNode.Name));
            throw InvalidTransactionException.FromPermanentlyRejectedTransactionError(submissionResult.PermanentlyRejectedReason);
        }

        if (submissionResult.AlreadyKnown)
        {
            await _observers.ForEachAsync(x => x.ObserveTransactionSubmissionToGatewayOutcome(TransactionSubmissionOutcome.DuplicateSubmission, targetNode.Name));
            return new GatewayModel.TransactionSubmitResponse(duplicate: true);
        }

        await _observers.ForEachAsync(x => x.ObserveTransactionSubmissionToGatewayOutcome(TransactionSubmissionOutcome.SubmittedToNetwork, targetNode.Name));
        return new GatewayModel.TransactionSubmitResponse(duplicate: false);
    }

    private async Task CheckPendingTransactionEpochValidity(GatewayModel.LedgerState ledgerState, ToolkitModel.NotarizedTransaction parsedTransaction)
    {
        var header = parsedTransaction.SignedIntent().Intent().Header();
        var currentEpoch = (ulong)ledgerState.Epoch;

        if (header.startEpochInclusive > currentEpoch + 1)
        {
            await _observers.ForEachAsync(x => x.ObserveTransactionSubmissionToGatewayOutcome(TransactionSubmissionOutcome.StartEpochInFuture));
            throw InvalidTransactionException.StartEpochTooFarInFuture(currentEpoch, header.startEpochInclusive);
        }

        if (header.endEpochExclusive <= currentEpoch)
        {
            await _observers.ForEachAsync(x => x.ObserveTransactionSubmissionToGatewayOutcome(TransactionSubmissionOutcome.EndEpochInPast));
            throw InvalidTransactionException.NoLongerValid(currentEpoch, header.endEpochExclusive);
        }
    }

    private async Task<ToolkitModel.NotarizedTransaction> HandlePreSubmissionParseTransaction(byte[] notarizedTransactionBytes)
    {
        try
        {
            var notarizedTransaction = ToolkitModel.NotarizedTransaction.Decompile(notarizedTransactionBytes);
            notarizedTransaction.StaticallyValidate(ToolkitModel.ValidationConfig.Default((await _networkConfigurationProvider.GetNetworkConfiguration()).Id));
            return notarizedTransaction;
        }
        catch (ToolkitModel.RadixEngineToolkitException.TransactionValidationFailed ex)
        {
            await _observers.ForEachAsync(x => x.ObserveTransactionSubmissionToGatewayOutcome(TransactionSubmissionOutcome.ParseFailedStaticallyInvalid));
            throw InvalidTransactionException.FromStaticallyInvalid(ex.error);
        }
        catch (ToolkitModel.RadixEngineToolkitException ex)
        {
            await _observers.ForEachAsync(x => x.ObserveTransactionSubmissionToGatewayOutcome(TransactionSubmissionOutcome.ParseFailedIncorrectFormat));
            _logger.LogWarning(ex, "Corrupted notarized tx bytes");
            throw InvalidTransactionException.FromUnsupportedPayloadType();
        }
        catch (Exception ex)
        {
            await _observers.ForEachAsync(x => x.ObserveTransactionSubmissionToGatewayOutcome(TransactionSubmissionOutcome.ParseFailedOtherError));
            _logger.LogWarning(ex, "Exception when parsing / validating submitted transaction");
            throw;
        }
    }
}
