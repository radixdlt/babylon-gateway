using RadixDlt.CoreApiSdk.Model;
using System;
using System.Collections.Generic;

namespace RadixDlt.NetworkGateway.IntegrationTests.Builders;

public class TransactionReceiptBuilder : BuilderBase<TransactionReceipt>
{
    private TransactionStatus _transactionStatus;
    private FeeSummary? _feeSummary;
    private StateUpdates? _stateUpdates;

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
            status: _transactionStatus,
            feeSummary: _feeSummary,
            stateUpdates: _stateUpdates,
            output: new List<SborData>(),
            errorMessage: "error"
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
            loanFullyRepaid: true,
            costUnitLimit: 10000000,
            costUnitConsumed: 0,
            costUnitPriceAttos: "1000000000000",
            tipPercentage: 0,
            xrdBurnedAttos: "0",
            xrdTippedAttos: "0"
        );
    }
}
