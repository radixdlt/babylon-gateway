using RadixDlt.CoreApiSdk.Model;
using System;
using System.Collections.Generic;

namespace RadixDlt.NetworkGateway.IntegrationTests.Builders;

public class TransactionReceiptBuilder : BuilderBase<TransactionReceipt>
{
    private FeeSummary? _feeSummary;
    private StateUpdates? _stateUpdates;
    private TransactionStatus _transactionStatus;

    public override TransactionReceipt Build()
    {
        if (_feeSummary == null)
        {
            _feeSummary = GetDefaultFeeSummary();
        }

        if (_stateUpdates == null)
        {
            throw new ArgumentException("No StateUpdates found.");
        }

        return new TransactionReceipt(
            _transactionStatus,
            _feeSummary,
            _stateUpdates,
            new List<SborData>()
        );
    }

    public TransactionReceiptBuilder WithFeeSummary(FeeSummary feeSummary)
    {
        _feeSummary = feeSummary;

        return this;
    }

    public TransactionReceiptBuilder WithStateUpdates(StateUpdates stateUpdates)
    {
        _stateUpdates = stateUpdates;

        return this;
    }

    public TransactionReceiptBuilder WithTransactionStatus(TransactionStatus transactionStatus)
    {
        _transactionStatus = transactionStatus;

        return this;
    }

    private FeeSummary GetDefaultFeeSummary()
    {
        return new FeeSummary(
            true,
            10000000,
            0,
            "1000000000000",
            0,
            "0",
            "0"
        );
    }
}
