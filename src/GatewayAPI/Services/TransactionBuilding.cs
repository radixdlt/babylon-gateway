using Common.Numerics;
using GatewayAPI.ApiSurface;
using Core = RadixCoreApi.GeneratedClient.Model;
using Gateway = RadixGatewayApi.Generated.Model;

namespace GatewayAPI.Services;

public static class TransactionBuilding
{
    public static readonly TokenAmount MinimumStake = TokenAmount.FromSubUnitsString("90000000000000000000");
    public static readonly int MaximumMessageLength = 255;
    public static readonly string OnlyValidGranularity = "1";

    public static Core.OperationGroup OperationGroupOf(params Core.Operation[] operations)
    {
        return new Core.OperationGroup(operations.ToList());
    }

    public static Core.Operation DebitOperation(this ValidatedAccountAddress fromAccount, ValidatedTokenAmount tokenAmount)
    {
        return ResourceOperation(fromAccount.ToEntityIdentifier(), tokenAmount.ToNegativeResourceAmount());
    }

    public static Core.Operation CreditOperation(this ValidatedAccountAddress toAccount, ValidatedTokenAmount tokenAmount)
    {
        return ResourceOperation(toAccount.ToEntityIdentifier(), tokenAmount.ToResourceAmount());
    }

    public static Core.Operation CreditPendingStakeVaultOperation(
        ValidatedAccountAddress toAccount,
        ValidatedValidatorAddress validatorAddress,
        ValidatedTokenAmount tokenAmount
    )
    {
        return ResourceOperation(toAccount.ToPreparedStakesEntityIdentifier(validatorAddress), tokenAmount.ToResourceAmount());
    }

    public static Core.Operation DebitStakeVaultOperation(
        ValidatedAccountAddress account,
        ValidatedValidatorAddress validatorAddress,
        TokenAmount stakeUnitAmount
    )
    {
        return ResourceOperation(account.ToEntityIdentifier(), (-stakeUnitAmount).AsStakeUnitAmount(validatorAddress));
    }

    public static Core.Operation CreditPendingUnStakeVaultOperation(
        ValidatedAccountAddress account,
        ValidatedValidatorAddress validatorAddress,
        TokenAmount stakeUnitAmount
    )
    {
        return ResourceOperation(account.ToPreparedUnstakesEntityIdentifier(), stakeUnitAmount.AsStakeUnitAmount(validatorAddress));
    }

    public static Core.Operation ClaimAddressOperation(this ValidatedResourceAddress resourceAddress)
    {
        return DownDataOperation(resourceAddress.ToEntityIdentifier(), new Core.UnclaimedRadixEngineAddress());
    }

    public static Core.Operation CreateTokenData(this ValidatedResourceAddress resourceAddress, Core.TokenData tokenData)
    {
        return UpDataOperation(resourceAddress.ToEntityIdentifier(), tokenData);
    }

    public static Core.Operation CreateTokenMetadata(this ValidatedResourceAddress resourceAddress, Core.TokenMetadata tokenMetadata)
    {
        return UpDataOperation(resourceAddress.ToEntityIdentifier(), tokenMetadata);
    }

    public static Core.EntityIdentifier ToEntityIdentifier(this ValidatedAccountAddress accountAddress)
    {
        return new Core.EntityIdentifier(
            address: accountAddress.Address,
            subEntity: null
        );
    }

    public static Core.EntityIdentifier ToEntityIdentifier(this ValidatedResourceAddress resourceAddress)
    {
        return new Core.EntityIdentifier(
            address: resourceAddress.Rri,
            subEntity: null
        );
    }

    public static Core.EntityIdentifier ToPreparedStakesEntityIdentifier(
        this ValidatedAccountAddress accountAddress,
        ValidatedValidatorAddress validatedValidatorAddress
    )
    {
        return new Core.EntityIdentifier(
            address: accountAddress.Address,
            subEntity: new Core.SubEntity(
                "prepared_stakes",
                new Core.SubEntityMetadata(
                    validatorAddress: validatedValidatorAddress.Address
                )
            )
        );
    }

    public static Core.EntityIdentifier ToPreparedUnstakesEntityIdentifier(
        this ValidatedAccountAddress accountAddress
    )
    {
        return new Core.EntityIdentifier(
            address: accountAddress.Address,
            subEntity: new Core.SubEntity(
                "prepared_unstakes",
                metadata: null
            )
        );
    }

    public static Core.ResourceAmount AsStakeUnitAmount(
        this TokenAmount tokenAmount,
        ValidatedValidatorAddress validatorAddress
    )
    {
        return new Core.ResourceAmount(
            tokenAmount.ToSubUnitString(),
            new Core.StakeUnitResourceIdentifier(validatorAddress: validatorAddress.Address)
        );
    }

    public static Core.ResourceAmount ToResourceAmount(this ValidatedTokenAmount tokenAmount)
    {
        return new Core.ResourceAmount(
            tokenAmount.Amount.ToSubUnitString(),
            new Core.TokenResourceIdentifier(tokenAmount.Rri)
        );
    }

    public static Core.ResourceAmount ToNegativeResourceAmount(this ValidatedTokenAmount tokenAmount)
    {
        return new Core.ResourceAmount(
            (-tokenAmount.Amount).ToSubUnitString(),
            new Core.TokenResourceIdentifier(tokenAmount.Rri)
        );
    }

    private static Core.Operation ResourceOperation(
        Core.EntityIdentifier entityIdentifier,
        Core.ResourceAmount resourceAmount
    )
    {
        return new Core.Operation(
            "Resource",
            entityIdentifier,
            substate: null,
            amount: resourceAmount,
            data: null,
            metadata: null
        );
    }

    private static Core.Operation UpDataOperation(
        Core.EntityIdentifier entityIdentifier,
        Core.DataObject dataObject
    )
    {
        return new Core.Operation(
            "Data",
            entityIdentifier,
            substate: null,
            amount: null,
            data: new Core.Data(Core.Data.ActionEnum.CREATE, dataObject),
            metadata: null
        );
    }

    private static Core.Operation DownDataOperation(
        Core.EntityIdentifier entityIdentifier,
        Core.DataObject dataObject
    )
    {
        return new Core.Operation(
            "Data",
            entityIdentifier,
            substate: null,
            amount: null,
            data: new Core.Data(Core.Data.ActionEnum.DELETE, dataObject),
            metadata: null
        );
    }
}
