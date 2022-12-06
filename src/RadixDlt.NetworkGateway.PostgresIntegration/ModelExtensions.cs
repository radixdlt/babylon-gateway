using RadixDlt.NetworkGateway.Abstractions.Model;
using System;
using CoreModel = RadixDlt.CoreApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration;

internal static class ModelExtensions
{
    public static LedgerTransactionStatus ToModel(this CoreModel.TransactionStatus input)
    {
        return input switch
        {
            CoreModel.TransactionStatus.Succeeded => LedgerTransactionStatus.Succeeded,
            CoreModel.TransactionStatus.Failed => LedgerTransactionStatus.Failed,
            _ => throw new ArgumentOutOfRangeException(nameof(input), input, null),
        };
    }
}
