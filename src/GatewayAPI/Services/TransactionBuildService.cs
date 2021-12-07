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

using Common.Addressing;
using GatewayAPI.ApiSurface;
using GatewayAPI.CoreCommunications;
using GatewayAPI.Database;
using GatewayAPI.Exceptions;
using System.Globalization;
using Core = RadixCoreApi.GeneratedClient.Model;
using Gateway = RadixGatewayApi.Generated.Model;
using TokenAmount = Common.Numerics.TokenAmount;

namespace GatewayAPI.Services;

public interface ITransactionBuildService
{
    Task<Gateway.TransactionBuild> HandleBuildRequest(Gateway.TransactionBuildRequest request, Gateway.LedgerState ledgerState);

    Task<Gateway.TransactionFinalizeResponse> HandleFinalizeRequest(Gateway.TransactionFinalizeRequest request);

    Task<Gateway.TransactionSubmitResponse> HandleSubmitRequest(Gateway.TransactionSubmitRequest request);
}

public class TransactionBuildService : ITransactionBuildService
{
    private readonly IValidations _validations;
    private readonly IAccountQuerier _accountQuerier;
    private readonly ICoreApiHandler _coreApiHandler;
    private readonly INetworkConfigurationProvider _networkConfigurationProvider;

    public TransactionBuildService(
        IValidations validations,
        IAccountQuerier accountQuerier,
        ICoreApiHandler coreApiHandler,
        INetworkConfigurationProvider networkConfigurationProvider
    )
    {
        _validations = validations;
        _accountQuerier = accountQuerier;
        _coreApiHandler = coreApiHandler;
        _networkConfigurationProvider = networkConfigurationProvider;
    }

    public async Task<Gateway.TransactionBuild> HandleBuildRequest(Gateway.TransactionBuildRequest request, Gateway.LedgerState ledgerState)
    {
        // TODO - sum up total resources debited from account, and compare to account's balances of these.
        var coreBuildRequest = await ConvertBuildRequestToCoreRequest(request, ledgerState);

        // TODO - catch NotEnoughResource exception (given that this service thinks they have) and report not enough fees
        var coreBuildResponse = await _coreApiHandler.BuildTransaction(coreBuildRequest);

        var coreParseResponse = await _coreApiHandler.ParseTransaction(new Core.ConstructionParseRequest(
            networkIdentifier: _coreApiHandler.GetNetworkIdentifier(),
            transaction: coreBuildResponse.UnsignedTransaction,
            signed: false
        ));

        return new Gateway.TransactionBuild(
            fee: coreParseResponse.Metadata.Fee.AsApiTokenAmount(),
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
            transactionIdentifier: new Gateway.TransactionIdentifier(transactionHashResponse.TransactionIdentifier.Hash)
        );
    }

    public async Task<Gateway.TransactionSubmitResponse> HandleSubmitRequest(Gateway.TransactionSubmitRequest request)
    {
        var submitResponse = await HandleSubmission(
            _validations.ExtractValidHex("Signed transaction", request.SignedTransaction)
        );

        return new Gateway.TransactionSubmitResponse(
            transactionIdentifier: new Gateway.TransactionIdentifier(submitResponse.TransactionIdentifier.Hash)
        );
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
        catch (InternalInvalidSignatureException ex)
        {
            throw new InvalidSignatureException(gatewayRequest.Signature, ex.Message);
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
        catch (InternalTransactionSubstateIsNotUpException ex)
        {
            throw ex.ToInvalidTransactionException(signedTransaction.AsString);
        }
    }

    private async Task<Core.ConstructionBuildRequest> ConvertBuildRequestToCoreRequest(Gateway.TransactionBuildRequest request, Gateway.LedgerState ledgerState)
    {
        var feePayer = _validations.ExtractValidAccountAddress(request.FeePayer);
        var validatedMessage = _validations.ExtractOptionalValidHexOrNull("Message", request.Message);

        if (validatedMessage != null && validatedMessage.Bytes.Length > TransactionBuilding.MaximumMessageLength)
        {
            throw new MessageTooLongException(TransactionBuilding.MaximumMessageLength, validatedMessage.Bytes.Length);
        }

        return new Core.ConstructionBuildRequest(
            _coreApiHandler.GetNetworkIdentifier(),
            await MapActions(request.Actions, ledgerState),
            feePayer: feePayer.ToEntityIdentifier(),
            message: validatedMessage?.AsString,
            disableResourceAllocateAndDestroy: request.DisableTokenMintAndBurn
        );
    }

    private async Task<List<Core.OperationGroup>> MapActions(List<Gateway.Action> requestActions, Gateway.LedgerState ledgerState)
    {
        var operationGroups = new List<Core.OperationGroup>();
        foreach (var action in requestActions)
        {
            operationGroups.Add(await MapAction(action, ledgerState));
        }

        return operationGroups;
    }

    private async Task<Core.OperationGroup> MapAction(Gateway.Action action, Gateway.LedgerState ledgerState)
    {
        return action switch
        {
            Gateway.TransferTokens transferTokens => MapTransferTokens(transferTokens),
            Gateway.BurnTokens burnTokens => MapBurnTokens(burnTokens),
            Gateway.MintTokens mintTokens => MapMintTokens(mintTokens),
            Gateway.StakeTokens stakeTokens => MapStakeTokens(stakeTokens),
            Gateway.UnstakeTokens unstakeTokens => await MapUnstakeTokens(unstakeTokens, ledgerState),
            Gateway.CreateTokenDefinition createTokenDefinition => MapCreateTokenDefinition(createTokenDefinition),
            _ => throw new ArgumentOutOfRangeException(nameof(action), action, "Unhandled action type"),
        };
    }

    private Core.OperationGroup MapTransferTokens(Gateway.TransferTokens action)
    {
        var fromAccount = _validations.ExtractValidAccountAddress(action.FromAccount);
        var toAccount = _validations.ExtractValidAccountAddress(action.ToAccount);
        var tokenAmount = _validations.ExtractValidPositiveTokenAmount(action.Amount);

        if (fromAccount.Address == toAccount.Address)
        {
            throw new InvalidActionException(action, "Contains a transfer from/to the same address");
        }

        // TODO:NG-23 - Check for sufficient tokens
        return TransactionBuilding.OperationGroupOf(
            fromAccount.DebitOperation(tokenAmount),
            toAccount.CreditOperation(tokenAmount)
        );
    }

    private Core.OperationGroup MapBurnTokens(Gateway.BurnTokens action)
    {
        var fromAccount = _validations.ExtractValidAccountAddress(action.FromAccount);
        var tokenAmount = _validations.ExtractValidPositiveTokenAmount(action.Amount);

        // TODO:NG-23 - Check for Burn Auth, and sufficient tokens
        return TransactionBuilding.OperationGroupOf(
            fromAccount.DebitOperation(tokenAmount)
        );
    }

    private Core.OperationGroup MapMintTokens(Gateway.MintTokens action)
    {
        var toAccount = _validations.ExtractValidAccountAddress(action.ToAccount);
        var tokenAmount = _validations.ExtractValidPositiveTokenAmount(action.Amount);

        // TODO:NG-23 - Check for Mint Auth, and sufficient tokens
        return TransactionBuilding.OperationGroupOf(
            toAccount.CreditOperation(tokenAmount)
        );
    }

    private Core.OperationGroup MapStakeTokens(Gateway.StakeTokens action)
    {
        var fromAccount = _validations.ExtractValidAccountAddress(action.FromAccount);
        var toValidator = _validations.ExtractValidValidatorAddress(action.ToValidator);
        var tokenAmount = _validations.ExtractValidPositiveTokenAmount(action.Amount);

        if (tokenAmount.Amount < TransactionBuilding.MinimumStake)
        {
            throw new BelowMinimumStakeException(
                tokenAmount.AsApiTokenAmount(),
                TransactionBuilding.MinimumStake.AsApiTokenAmount(_networkConfigurationProvider.GetXrdTokenIdentifier())
            );
        }

        // TODO:NG-23 - Check for sufficient tokens; Check for allow delegation flag
        return TransactionBuilding.OperationGroupOf(
            fromAccount.DebitOperation(tokenAmount),
            TransactionBuilding.CreditPendingStakeVaultOperation(fromAccount, toValidator, tokenAmount)
        );
    }

    private async Task<Core.OperationGroup> MapUnstakeTokens(Gateway.UnstakeTokens action, Gateway.LedgerState ledgerState)
    {
        var validator = _validations.ExtractValidValidatorAddress(action.FromValidator);
        var account = _validations.ExtractValidAccountAddress(action.ToAccount);

        // We read in the stakeSnapshot at the given ledgerState so that the wallet can make the request against the given ledger state
        var stakeSnapshot = await _accountQuerier.GetStakeSnapshotAtState(account, validator, ledgerState);

        var stakeUnitsToUnstake = action switch
        {
            { Amount: { } xrdAmount, UnstakePercentage: 0 } => GetStakeUnitsToUnstakeGivenFixedXrdAmountRequested(validator, stakeSnapshot, xrdAmount),
            { Amount: null, UnstakePercentage: > 0 and var percentage } => GetStakeUnitsGivenUnstakePercentage(action, stakeSnapshot, percentage),
            _ => throw new InvalidActionException(action, "Only one of Amount or UnstakePercentage should be provided for an UnstakeTokens action"),
        };

        return TransactionBuilding.OperationGroupOf(
            TransactionBuilding.DebitStakeVaultOperation(account, validator, stakeUnitsToUnstake),
            TransactionBuilding.CreditPendingUnStakeVaultOperation(account, validator, stakeUnitsToUnstake)
        );
    }

    private TokenAmount GetStakeUnitsToUnstakeGivenFixedXrdAmountRequested(
        ValidatedValidatorAddress validator,
        AccountQuerier.CombinedStakeSnapshot stakeSnapshot,
        Gateway.TokenAmount xrdTokenAmount
    )
    {
        var requestedXrdToUnstake = _validations.ExtractValidPositiveXrdTokenAmount(xrdTokenAmount);

        var estimatedTotalXrdStaked = stakeSnapshot.ValidatorStakeSnapshot.EstimateXrdConversion(
            stakeSnapshot.AccountValidatorStakeSnapshot.TotalStakeUnits
        );

        if (requestedXrdToUnstake.Amount > estimatedTotalXrdStaked)
        {
            throw new NotEnoughTokensForUnstakeException(
                requestedXrdToUnstake.AsApiTokenAmount(),
                new Gateway.AccountStakeEntry(
                    validator.Address.AsValidatorIdentifier(),
                    estimatedTotalXrdStaked.AsApiTokenAmount(_networkConfigurationProvider.GetXrdTokenIdentifier())),
                new Gateway.AccountStakeEntry(
                    validator.Address.AsValidatorIdentifier(),
                    stakeSnapshot.AccountValidatorStakeSnapshot.TotalPreparedXrdStake.AsApiTokenAmount(_networkConfigurationProvider.GetXrdTokenIdentifier())
                )
            );
        }

        // If the amount to unstake is within 0.001 XRD, unstake everything to try to prevent the "dust" problem
        return (estimatedTotalXrdStaked - requestedXrdToUnstake.Amount) < TokenAmount.FromDecimalString("0.001")
            ? stakeSnapshot.AccountValidatorStakeSnapshot.TotalStakeUnits
            : (stakeSnapshot.AccountValidatorStakeSnapshot.TotalStakeUnits * requestedXrdToUnstake.Amount) / estimatedTotalXrdStaked;
    }

    private TokenAmount GetStakeUnitsGivenUnstakePercentage(Gateway.Action action, AccountQuerier.CombinedStakeSnapshot stakeSnapshot, decimal percentage)
    {
        if (percentage is not (> 0 and <= 100))
        {
            throw new InvalidActionException(action, "Percentage unstake must be > 0 and <= 100");
        }

        var proportionAsTokenAmount = TokenAmount.FromDecimalString((percentage / 100).ToString(CultureInfo.InvariantCulture));

        return (stakeSnapshot.AccountValidatorStakeSnapshot.TotalStakeUnits * proportionAsTokenAmount) / TokenAmount.OneFullUnit;
    }

    private Core.OperationGroup MapCreateTokenDefinition(Gateway.CreateTokenDefinition action)
    {
        var account = _validations.ExtractValidAccountAddress(action.ToAccount);
        var validatedSymbol = _validations.ExtractValidTokenSymbol(action.TokenProperties.Symbol).AsString;

        var resourceAddressStr = RadixBech32.GenerateResourceAddress(
            account.ByteAccountAddress.CompressedPublicKey,
            validatedSymbol,
            _networkConfigurationProvider.GetAddressHrps().ResourceHrpSuffix
        );

        var validatedGranularity = action.TokenProperties.Granularity;

        if (validatedGranularity != TransactionBuilding.OnlyValidGranularity)
        {
            throw new InvalidActionException(action, $"Only a granularity of {TransactionBuilding.OnlyValidGranularity} is currently supported");
        }

        var resourceAddress = _validations.ExtractValidResourceAddress(new Gateway.TokenIdentifier(resourceAddressStr));

        var tokenMetadata = new Core.TokenMetadata(
            symbol: validatedSymbol,
            /* The following properties aren't currently validated, beyond the cost of the bytes required to create them */
            name: action.TokenProperties.Name,
            description: action.TokenProperties.Description,
            url: action.TokenProperties.Url,
            iconUrl: action.TokenProperties.IconUrl
        );

        var validatedTokenSupply = _validations.ExtractValidPositiveTokenAmount(action.TokenSupply);

        if (validatedTokenSupply.Rri != resourceAddress.Rri)
        {
            throw new InvalidActionException(action, $"The token supply of the resource was against the rri {validatedTokenSupply.Rri} when it should have been {resourceAddress.Rri}");
        }

        return action.TokenProperties.IsSupplyMutable
            ? CreateMutableSupplyToken(action, resourceAddress, validatedTokenSupply, validatedGranularity, account, tokenMetadata)
            : CreateFixedSupplyToken(resourceAddress, validatedTokenSupply,  validatedGranularity, account, tokenMetadata);
    }

    private Core.OperationGroup CreateMutableSupplyToken(
        Gateway.Action action,
        ValidatedResourceAddress resourceAddress,
        ValidatedTokenAmount validatedTokenSupply,
        string validatedGranularity,
        ValidatedAccountAddress account,
        Core.TokenMetadata tokenMetadata
    )
    {
        if (!validatedTokenSupply.Amount.IsZero())
        {
            throw new InvalidActionException(action, $"The token supply of a new mutable supply token should be 0 (as it can be minted in follow up actions), but you provided {validatedTokenSupply.Amount}");
        }

        /* On node, token data construction starts by calling Syscall.READDR_CLAIM syscall for the symbol
            -- but we can't do that through the Core API so I think we can skip this? */
        return TransactionBuilding.OperationGroupOf(
            resourceAddress.ClaimAddressOperation(),
            resourceAddress.CreateTokenData(new Core.TokenData(
                granularity: validatedGranularity,
                isMutable: true,
                owner: account.ToEntityIdentifier()
            )),
            resourceAddress.CreateTokenMetadata(tokenMetadata)
        );
    }

    private Core.OperationGroup CreateFixedSupplyToken(
        ValidatedResourceAddress resourceAddress,
        ValidatedTokenAmount validatedTokenSupply,
        string validatedGranularity,
        ValidatedAccountAddress account,
        Core.TokenMetadata tokenMetadata
    )
    {
        /* On node, token data construction starts by calling Syscall.READDR_CLAIM syscall for the symbol
            -- but we can't do that through the Core API so I think we can skip this? */
        return TransactionBuilding.OperationGroupOf(
            resourceAddress.ClaimAddressOperation(),
            resourceAddress.CreateTokenData(new Core.TokenData(
                granularity: validatedGranularity,
                isMutable: false,
                owner: null
            )),
            /* For a fixed token supply, immediately pass the created supply to the owner */
            account.CreditOperation(
                new ValidatedTokenAmount(resourceAddress.Rri, validatedTokenSupply.Amount, resourceAddress.ResourceAddress)
            ),
            resourceAddress.CreateTokenMetadata(tokenMetadata)
        );
    }
}
