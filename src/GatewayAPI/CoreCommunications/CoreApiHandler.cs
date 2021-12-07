using Common.Extensions;
using GatewayAPI.ApiSurface;
using GatewayAPI.Configuration;
using GatewayAPI.Exceptions;
using GatewayAPI.Services;
using Newtonsoft.Json;
using RadixCoreApi.GeneratedClient.Client;
using RadixCoreApi.GeneratedClient.Model;
using BelowMinimumStakeError = RadixCoreApi.GeneratedClient.Model.BelowMinimumStakeError;
using Gateway = RadixGatewayApi.Generated.Model;
using InternalServerError = RadixCoreApi.GeneratedClient.Model.InternalServerError;
using InvalidPublicKeyError = RadixCoreApi.GeneratedClient.Model.InvalidPublicKeyError;
using InvalidSignatureError = RadixCoreApi.GeneratedClient.Model.InvalidSignatureError;
using InvalidTransactionError = RadixCoreApi.GeneratedClient.Model.InvalidTransactionError;
using MessageTooLongError = RadixCoreApi.GeneratedClient.Model.MessageTooLongError;
using NetworkIdentifier = RadixCoreApi.GeneratedClient.Model.NetworkIdentifier;
using NetworkNotSupportedError = RadixCoreApi.GeneratedClient.Model.NetworkNotSupportedError;
using TransactionNotFoundError = RadixCoreApi.GeneratedClient.Model.TransactionNotFoundError;

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

    public CoreApiHandler(INetworkGatewayConfiguration configuration, INetworkConfigurationProvider networkConfigurationProvider, HttpClient httpClient)
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

            // A lot of these are null because the Gateway service should have checked these -- if these exceptions
            // get thrown it's likely a problem with the Gateway service not doing enough validation
            KnownGatewayErrorException? newError = coreError.Details switch
            {
                // ReSharper disable UnusedVariable
                AboveMaximumValidatorFeeIncreaseError error => InvalidRequestException.FromOtherError(
                    $"You attempted to increase validator fee by {error.AttemptedValidatorFeeIncrease}, larger than the maximum of {error.MaximumValidatorFeeIncrease}"
                ),
                BelowMinimumStakeError error => new BelowMinimumStakeException(
                    requestedAmount: error.MinimumStake.AsApiTokenAmount(),
                    minimumAmount: error.MinimumStake.AsApiTokenAmount()
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
                InvalidSignatureError error => throw new InternalInvalidSignatureException(error.InvalidSignature), // We throw a marker exception because we don't have the required data to construct the full exception
                InvalidSubEntityError error => null,
                InvalidTransactionError error => null,
                InvalidTransactionHashError error => null,
                MessageTooLongError error => new MessageTooLongException(error.MaximumMessageLength, error.AttemptedMessageLength),
                NetworkNotSupportedError error => null,
                NotEnoughResourcesError error => null, // Not specific enough - rely on Gateway handling
                NotValidatorOwnerError error => null, // Not specific enough - rely on Gateway handling
                PublicKeyNotSupportedError error => new InvalidPublicKeyException(
                    new Gateway.PublicKey(error.UnsupportedPublicKey.Hex),
                    "Public key is not supported"
                ),
                ResourceDepositOperationNotSupportedByEntityError error => null,
                ResourceWithdrawOperationNotSupportedByEntityError error => null,
                StateIdentifierNotFoundError error => null,
                SubstateDependencyNotFoundError error => throw new InternalTransactionSubstateIsNotUpException(
                    error.SubstateIdentifierNotFound.Identifier
                ),
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

    private static ICoreApiProvider ChooseCoreApiProvider(INetworkGatewayConfiguration configuration, HttpClient httpClient)
    {
        var chosenNode = configuration.GetCoreNodes()
            .Where(n => n.IsEnabled && !string.IsNullOrWhiteSpace(n.CoreApiAddress))
            .GetRandomBy(n => (double)n.RequestWeighting);

        return new CoreApiProvider(chosenNode, httpClient);
    }
}
