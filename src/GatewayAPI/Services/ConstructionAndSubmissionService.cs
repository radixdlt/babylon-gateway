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

using Common.Numerics;
using GatewayAPI.ApiSurface;
using GatewayAPI.CoreCommunications;
using GatewayAPI.Database;
using GatewayAPI.Exceptions;
using Core = RadixCoreApi.GeneratedClient.Model;
using Gateway = RadixGatewayApi.Generated.Model;

namespace GatewayAPI.Services;

public interface IConstructionAndSubmissionService
{
    Task<Gateway.TransactionBuild> HandleBuildRequest(Gateway.TransactionBuildRequest request, Gateway.LedgerState ledgerState);

    Task<Gateway.TransactionFinalizeResponse> HandleFinalizeRequest(Gateway.TransactionFinalizeRequest request);

    Task<Gateway.TransactionSubmitResponse> HandleSubmitRequest(Gateway.TransactionSubmitRequest request);
}

public class ConstructionAndSubmissionService : IConstructionAndSubmissionService
{
    private readonly IValidations _validations;
    private readonly IAccountQuerier _accountQuerier;
    private readonly IValidatorQuerier _validatorQuerier;
    private readonly ITokenQuerier _tokenQuerier;
    private readonly ICoreApiHandler _coreApiHandler;
    private readonly INetworkConfigurationProvider _networkConfigurationProvider;

    public ConstructionAndSubmissionService(
        IValidations validations,
        IAccountQuerier accountQuerier,
        IValidatorQuerier validatorQuerier,
        ITokenQuerier tokenQuerier,
        ICoreApiHandler coreApiHandler,
        INetworkConfigurationProvider networkConfigurationProvider
    )
    {
        _validations = validations;
        _accountQuerier = accountQuerier;
        _validatorQuerier = validatorQuerier;
        _tokenQuerier = tokenQuerier;
        _coreApiHandler = coreApiHandler;
        _networkConfigurationProvider = networkConfigurationProvider;
    }

    public async Task<Gateway.TransactionBuild> HandleBuildRequest(Gateway.TransactionBuildRequest request, Gateway.LedgerState ledgerState)
    {
        var coreBuildResponse = await BuildTransaction(request, ledgerState);

        var coreParseResponse = await _coreApiHandler.ParseTransaction(new Core.ConstructionParseRequest(
            networkIdentifier: _coreApiHandler.GetNetworkIdentifier(),
            transaction: coreBuildResponse.UnsignedTransaction,
            signed: false
        ));

        return new Gateway.TransactionBuild(
            fee: coreParseResponse.Metadata.Fee.AsGatewayTokenAmount(),
            unsignedTransaction: coreBuildResponse.UnsignedTransaction,
            payloadToSign: coreBuildResponse.PayloadToSign
        );
    }

    public async Task<Gateway.TransactionFinalizeResponse> HandleFinalizeRequest(Gateway.TransactionFinalizeRequest request)
    {
        var coreFinalizeResponse = await HandleCoreFinalizeRequest(request, new Core.ConstructionFinalizeRequest(
            _coreApiHandler.GetNetworkIdentifier(),
            unsignedTransaction: _validations.ExtractValidHex("Unsigned transaction", request.UnsignedTransaction).AsString,
            signature: new Core.Signature(
                publicKey: new Core.PublicKey(
                    _validations.ExtractValidPublicKey(request.Signature.PublicKey).AsString
                ),
                bytes: _validations.ExtractValidHex("Signature Bytes", request.Signature.Bytes).AsString
            )
        ));

        var transactionHashResponse = await _coreApiHandler.GetTransactionHash(new Core.ConstructionHashRequest(
            _coreApiHandler.GetNetworkIdentifier(),
            coreFinalizeResponse.SignedTransaction
        ));

        if (request.Submit)
        {
            await HandleSubmission(
                _validations.ExtractValidHex("Signed transaction", coreFinalizeResponse.SignedTransaction)
            );
        }

        return new Gateway.TransactionFinalizeResponse(
            signedTransaction: coreFinalizeResponse.SignedTransaction,
            transactionIdentifier: transactionHashResponse.TransactionIdentifier.AsGatewayTransactionIdentifier()
        );
    }

    public async Task<Gateway.TransactionSubmitResponse> HandleSubmitRequest(Gateway.TransactionSubmitRequest request)
    {
        var submitResponse = await HandleSubmission(
            _validations.ExtractValidHex("Signed transaction", request.SignedTransaction)
        );

        return new Gateway.TransactionSubmitResponse(
            transactionIdentifier: submitResponse.TransactionIdentifier.AsGatewayTransactionIdentifier()
        );
    }

    private async Task<Core.ConstructionBuildResponse> BuildTransaction(Gateway.TransactionBuildRequest request, Gateway.LedgerState ledgerState)
    {
        var feePayer = _validations.ExtractValidAccountAddress(request.FeePayer);
        var validatedMessage = _validations.ExtractOptionalValidHexOrNull("Message", request.Message);

        if (validatedMessage != null && validatedMessage.Bytes.Length > TransactionBuilding.MaximumMessageLength)
        {
            throw new MessageTooLongException(TransactionBuilding.MaximumMessageLength, validatedMessage.Bytes.Length);
        }

        var transactionBuilder = new TransactionBuilder(
            _validations,
            _accountQuerier,
            _validatorQuerier,
            _tokenQuerier,
            _networkConfigurationProvider,
            ledgerState,
            feePayer
        );

        var mappedTransaction = await transactionBuilder.MapAndValidateActions(request.Actions);

        var coreBuildRequest = new Core.ConstructionBuildRequest(
            _coreApiHandler.GetNetworkIdentifier(),
            mappedTransaction.OperationGroups,
            feePayer: feePayer.ToEntityIdentifier(),
            message: validatedMessage?.AsString,
            disableResourceAllocateAndDestroy: request.DisableTokenMintAndBurn
        );

        try
        {
            return await _coreApiHandler.BuildTransaction(coreBuildRequest);
        }
        catch (WrappedCoreApiException<Core.NotEnoughResourcesError> notEnoughResourcesException)
        {
            var xrdAddress = _networkConfigurationProvider.GetXrdAddress();

            if (notEnoughResourcesException.Error.Available.ResourceIdentifier is not Core.TokenResourceIdentifier tokenResourceIdentifier)
            {
                throw;
            }

            if (tokenResourceIdentifier.Rri != _networkConfigurationProvider.GetXrdAddress())
            {
                throw;
            }

            var reportedAvailableXrd = TokenAmount.FromSubUnitsString(notEnoughResourcesException.Error.Available.Value);
            var beforeXrdBalance = mappedTransaction.BeforeBalances.GetValueOrDefault(xrdAddress);

            if (reportedAvailableXrd != beforeXrdBalance)
            {
                throw;
            }

            var attemptedToTakeXrd = TokenAmount.FromSubUnitsString(notEnoughResourcesException.Error.AttemptedToTake.Value);
            var xrdFeePayerBalanceChangeInTransaction = mappedTransaction.BalanceChanges.GetValueOrDefault(xrdAddress);

            // Core API is complaining about the amount of XRD they have, but agrees on how much they have on balance.
            // We already validated they have enough for transfers, so the difference must be the fee -- which they don't have.

            // NB: attemptedToTake = -totalAccountDebit = -(xrdChangeInTransaction - calculatedFee) = -xrdChangeInTransaction + calculatedFee. Rearranging:
            var calculatedFee = attemptedToTakeXrd + xrdFeePayerBalanceChangeInTransaction;
            var availableAmount = beforeXrdBalance + xrdFeePayerBalanceChangeInTransaction;

            throw new NotEnoughNativeTokensForFeeException(
                requiredAmount: calculatedFee.AsGatewayTokenAmount(_networkConfigurationProvider.GetXrdTokenIdentifier()),
                availableAmount: availableAmount.AsGatewayTokenAmount(_networkConfigurationProvider.GetXrdTokenIdentifier())
            );
        }
    }

    private async Task<Core.ConstructionFinalizeResponse> HandleCoreFinalizeRequest(
        Gateway.TransactionFinalizeRequest gatewayRequest,
        Core.ConstructionFinalizeRequest request
    )
    {
        try
        {
            return await _coreApiHandler.FinalizeTransaction(request);
        }
        catch (WrappedCoreApiException<Core.InvalidSignatureError>)
        {
            throw new InvalidSignatureException(gatewayRequest.Signature);
        }
    }

    private async Task<Core.ConstructionSubmitResponse> HandleSubmission(
        ValidatedHex signedTransaction
    )
    {
        // TODO:NG-35 - Support for saving transactions as pending, and automated retry (will need to GetHash first before submission)
        try
        {
            return await _coreApiHandler.SubmitTransaction(new Core.ConstructionSubmitRequest(
                _coreApiHandler.GetNetworkIdentifier(),
                signedTransaction.AsString
            ));
        }
        catch (WrappedCoreApiException<Core.SubstateDependencyNotFoundError> ex)
        {
            throw InvalidTransactionException.FromSubstateDependencyNotFoundError(signedTransaction.AsString, ex.Error);
        }
    }
}
