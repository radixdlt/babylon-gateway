using System.Diagnostics;
using System.Linq;
using CoreModel = RadixDlt.CoreApiSdk.Model;
using GatewayModel = RadixDlt.NetworkGateway.GatewayApiSdk.Model;

namespace RadixDlt.NetworkGateway.GatewayApi;

internal static class GatewayModelExtensions
{
    public static GatewayModel.TransactionCommittedOutcomeResponseFungibleChanges ToGatewayModel(this CoreModel.LtsEntityFungibleBalanceChanges input)
    {
        var feeBalanceChanges = input.FeeBalanceChanges
            .Select(x => new GatewayModel.TransactionCommittedOutcomeResponseFungibleFeeBalanceChange(x.Type.ToGatewayModel(), x.ResourceAddress, x.BalanceChange))
            .ToList();

        var nonFeeBalanceChanges = input.NonFeeBalanceChanges
            .Select(x => new GatewayModel.TransactionCommittedOutcomeResponseFungibleBalanceChange(x.ResourceAddress, x.BalanceChange))
            .ToList();

        return new GatewayModel.TransactionCommittedOutcomeResponseFungibleChanges(input.EntityAddress, feeBalanceChanges, nonFeeBalanceChanges);
    }

    public static GatewayModel.TransactionCommittedOutcomeResponseNonFungibleChanges ToGatewayModel(this CoreModel.LtsEntityNonFungibleBalanceChanges input)
    {
        return new GatewayModel.TransactionCommittedOutcomeResponseNonFungibleChanges(input.EntityAddress, input.ResourceAddress, input.Added, input.Removed);
    }

    private static GatewayModel.LtsFeeFungibleResourceBalanceChangeType ToGatewayModel(this CoreModel.LtsFeeFungibleResourceBalanceChangeType input)
    {
        return input switch
        {
            CoreModel.LtsFeeFungibleResourceBalanceChangeType.FeePayment => GatewayModel.LtsFeeFungibleResourceBalanceChangeType.FeePayment,
            CoreModel.LtsFeeFungibleResourceBalanceChangeType.FeeDistributed => GatewayModel.LtsFeeFungibleResourceBalanceChangeType.FeeDistributed,
            CoreModel.LtsFeeFungibleResourceBalanceChangeType.TipDistributed => GatewayModel.LtsFeeFungibleResourceBalanceChangeType.TipDistributed,
            CoreModel.LtsFeeFungibleResourceBalanceChangeType.RoyaltyDistributed => GatewayModel.LtsFeeFungibleResourceBalanceChangeType.RoyaltyDistributed,
            _ => throw new UnreachableException($"Didn't expect {input} value"),
        };
    }
}
