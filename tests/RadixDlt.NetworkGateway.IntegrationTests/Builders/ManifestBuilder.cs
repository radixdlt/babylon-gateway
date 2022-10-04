using System.Collections.Generic;

namespace RadixDlt.NetworkGateway.IntegrationTests.Builders;

public class ManifestBuilder : BuilderBase<string>
{
    private readonly List<string> _instructions = new();

    public override string Build()
    {
        return string.Join(";\n", _instructions);
    }

    public ManifestBuilder WithLockFeeMethod(string componentAddress, string lockFee)
    {
        _instructions.Add($"CALL_METHOD ComponentAddress(\"{componentAddress}\") \"lock_fee\" Decimal(\"{lockFee}\")");

        return this;
    }

    public ManifestBuilder WithWithdrawByAmountMethod(string componentAddress, string withdrawByAmount, string fromResourceAddress)
    {
        _instructions.Add($"CALL_METHOD ComponentAddress(\"{componentAddress}\") \"withdraw_by_amount\" Decimal(\"{withdrawByAmount}\") ResourceAddress (\"{fromResourceAddress})\"");

        return this;
    }

    public ManifestBuilder WithTakeFromWorktopByAmountMethod(string resourceAddress, string amount, string bucketName)
    {
        _instructions.Add($"TAKE_FROM_WORKTOP_BY_AMOUNT Decimal(\"{amount}\") ResourceAddress(\"{resourceAddress}\") Bucket(\"{bucketName}\")");

        return this;
    }

    public ManifestBuilder WithDepositToAccountMethod(string accountAddress, string bucketName)
    {
        _instructions.Add($"CALL_METHOD ComponentAddress(\"{accountAddress}\") \"deposit\" Bucket(\"{bucketName}\")");

        return this;
    }
}
