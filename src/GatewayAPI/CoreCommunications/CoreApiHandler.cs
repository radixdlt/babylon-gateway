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

using Common.Extensions;
using GatewayAPI.ApiSurface;
using GatewayAPI.Configuration;
using GatewayAPI.Exceptions;
using GatewayAPI.Services;
using Newtonsoft.Json;
using RadixCoreApi.Generated.Client;
using RadixCoreApi.Generated.Model;
using BelowMinimumStakeError = RadixCoreApi.Generated.Model.BelowMinimumStakeError;
using Gateway = RadixGatewayApi.Generated.Model;
using InternalServerError = RadixCoreApi.Generated.Model.InternalServerError;
using InvalidPublicKeyError = RadixCoreApi.Generated.Model.InvalidPublicKeyError;
using InvalidSignatureError = RadixCoreApi.Generated.Model.InvalidSignatureError;
using InvalidTransactionError = RadixCoreApi.Generated.Model.InvalidTransactionError;
using MessageTooLongError = RadixCoreApi.Generated.Model.MessageTooLongError;
using NetworkIdentifier = RadixCoreApi.Generated.Model.NetworkIdentifier;
using NetworkNotSupportedError = RadixCoreApi.Generated.Model.NetworkNotSupportedError;
using TransactionNotFoundError = RadixCoreApi.Generated.Model.TransactionNotFoundError;

namespace GatewayAPI.CoreCommunications;

public interface ICoreApiHandler
{
    NetworkIdentifier GetNetworkIdentifier();

    Task<ConstructionBuildResponse> BuildTransaction(ConstructionBuildRequest request);

    Task<ConstructionParseResponse> ParseTransaction(ConstructionParseRequest request);

    Task<ConstructionFinalizeResponse> FinalizeTransaction(ConstructionFinalizeRequest request);

    Task<ConstructionHashResponse> GetTransactionHash(ConstructionHashRequest request);

    Task<ConstructionSubmitResponse> SubmitTransaction(ConstructionSubmitRequest request);
}

/// <summary>
/// This should be Scoped to the request, so it picks up a fresh HttpClient per request.
/// </summary>
public class CoreApiHandler : ICoreApiHandler
{
    private readonly INetworkConfigurationProvider _networkConfigurationProvider;
    private readonly ICoreApiProvider _coreApiProvider;

    public CoreApiHandler(IGatewayApiConfiguration configuration, INetworkConfigurationProvider networkConfigurationProvider, HttpClient httpClient)
    {
        _networkConfigurationProvider = networkConfigurationProvider;
        _coreApiProvider = ChooseCoreApiProvider(configuration, httpClient);
    }

    public NetworkIdentifier GetNetworkIdentifier()
    {
        return new NetworkIdentifier(_networkConfigurationProvider.GetNetworkName());
    }

    public async Task<ConstructionBuildResponse> BuildTransaction(ConstructionBuildRequest request)
    {
        return await TranslateCoreApiErrors(() => _coreApiProvider.ConstructionApi.ConstructionBuildPostAsync(request));
    }

    public async Task<ConstructionParseResponse> ParseTransaction(ConstructionParseRequest request)
    {
        return await TranslateCoreApiErrors(() => _coreApiProvider.ConstructionApi.ConstructionParsePostAsync(request));
    }

    public async Task<ConstructionFinalizeResponse> FinalizeTransaction(ConstructionFinalizeRequest request)
    {
        return await TranslateCoreApiErrors(() => _coreApiProvider.ConstructionApi.ConstructionFinalizePostAsync(request));
    }

    public async Task<ConstructionHashResponse> GetTransactionHash(ConstructionHashRequest request)
    {
        return await TranslateCoreApiErrors(() => _coreApiProvider.ConstructionApi.ConstructionHashPostAsync(request));
    }

    public async Task<ConstructionSubmitResponse> SubmitTransaction(ConstructionSubmitRequest request)
    {
        return await TranslateCoreApiErrors(() => _coreApiProvider.ConstructionApi.ConstructionSubmitPostAsync(request));
    }

    private static async Task<T> TranslateCoreApiErrors<T>(Func<Task<T>> requestAction)
    {
        try
        {
            return await requestAction();
        }
        catch (ApiException apiException)
        {
            var coreError = ExtractUpstreamGatewayErrorResponse(apiException.ErrorContent?.ToString());
            if (coreError == null)
            {
                throw;
            }

            // General rules here:
            // * If the error shouldn't happen / shouldn't be handled => use null (to rethrow the apiException)
            //   most errors fall into this category - because we shouldn't be sending invalid requests upstream!
            // * If the error is definitely a client error => map straight to the corresponding client error
            // * If the error doesn't have enough information, or we need to handle it specially, wrap it so that
            //   we can catch is as a typed error further up the call stack.
            KnownGatewayErrorException? newError = coreError.Details switch
            {
                // ReSharper disable UnusedVariable
                AboveMaximumValidatorFeeIncreaseError error => InvalidRequestException.FromOtherError(
                    $"You attempted to increase validator fee by {error.AttemptedValidatorFeeIncrease}, larger than the maximum of {error.MaximumValidatorFeeIncrease}"
                ),
                BelowMinimumStakeError error => new BelowMinimumStakeException(
                    requestedAmount: error.MinimumStake.AsGatewayTokenAmount(),
                    minimumAmount: error.MinimumStake.AsGatewayTokenAmount()
                ), // Should have already detected this, but rethrow anyway
                DataObjectNotSupportedByEntityError error => null,
                FeeConstructionError error => new CouldNotConstructFeesException(error.Attempts),
                InternalServerError error => null,
                InvalidAddressError error => null, // Not specific enough - rely on Gateway handling
                InvalidDataObjectError error => null,
                InvalidFeePayerEntityError error => null,
                InvalidHexError error => null,
                InvalidJsonError error => null,
                InvalidPartialStateIdentifierError error => null,
                InvalidPublicKeyError error => throw new InvalidPublicKeyException(
                    new Gateway.PublicKey(error.InvalidPublicKey.Hex),
                    "Invalid public key"
                ),
                InvalidSignatureError error => throw WrappedCoreApiException.Of(apiException, error), // Handle in ConstructionService when we have the required data to construct the full exception
                InvalidSubEntityError error => null,
                InvalidTransactionError error => null,
                InvalidTransactionHashError error => null,
                MessageTooLongError error => new MessageTooLongException(error.MaximumMessageLength, error.AttemptedMessageLength),
                NetworkNotSupportedError error => null,
                NotEnoughResourcesError error => throw WrappedCoreApiException.Of(apiException, error), // Handle in ConstructionService
                NotValidatorOwnerError error => null, // Not specific enough - rely on Gateway handling
                PublicKeyNotSupportedError error => new InvalidPublicKeyException(
                    new Gateway.PublicKey(error.UnsupportedPublicKey.Hex),
                    "Public key is not supported"
                ),
                ResourceDepositOperationNotSupportedByEntityError error => null,
                ResourceWithdrawOperationNotSupportedByEntityError error => null,
                StateIdentifierNotFoundError error => null,
                SubstateDependencyNotFoundError error => throw WrappedCoreApiException.Of(apiException, error), // Handle in ConstructionService
                TransactionNotFoundError error => new TransactionNotFoundException(
                    new Gateway.TransactionIdentifier(error.TransactionIdentifier.Hash)
                ),
                _ => null,
            };

            // ReSharper restore UnusedVariable
            if (newError != null)
            {
                throw newError;
            }

            throw; // Rethrow unknown error to be handled as an unhandled Core API exception in the ExceptionHandler
        }
    }

    private static UnexpectedError? ExtractUpstreamGatewayErrorResponse(string? upstreamErrorResponse)
    {
        try
        {
            return string.IsNullOrWhiteSpace(upstreamErrorResponse) ? null : JsonConvert.DeserializeObject<UnexpectedError>(upstreamErrorResponse);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static ICoreApiProvider ChooseCoreApiProvider(IGatewayApiConfiguration configuration, HttpClient httpClient)
    {
        var chosenNode = configuration.GetCoreNodes()
            .Where(n => n.IsEnabled && !string.IsNullOrWhiteSpace(n.CoreApiAddress))
            .GetRandomBy(n => (double)n.RequestWeighting);

        return new CoreApiProvider(chosenNode, httpClient);
    }
}
