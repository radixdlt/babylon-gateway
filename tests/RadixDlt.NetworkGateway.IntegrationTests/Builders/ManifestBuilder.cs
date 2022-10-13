using System.Collections.Generic;

namespace RadixDlt.NetworkGateway.IntegrationTests.Builders;

public class ManifestBuilder : BuilderBase<string>
{
    private readonly List<string> _instructions = new();

    public override string Build()
    {
        return string.Join(";\n", _instructions);
    }

    public ManifestBuilder WithCallMethod(string componentAddress, string methodName, string[]? args = null)
    {
        var instruction = $"CALL_METHOD ComponentAddress(\"{componentAddress}\") \"{methodName}\"";

        if (args != null)
        {
            instruction += $" {string.Join(" ", args)}";
        }

        _instructions.Add(instruction);

        return this;
    }

    public ManifestBuilder WithLockFeeMethod(string componentAddress, string lockFee)
    {
        _instructions.Add($"CALL_METHOD ComponentAddress(\"{componentAddress}\") \"lock_fee\" Decimal(\"{lockFee}\")");

        return this;
    }

    public ManifestBuilder WithWithdrawByAmountMethod(string componentAddress, string withdrawByAmount, string fromResourceAddress)
    {
        _instructions.Add(
            $"CALL_METHOD ComponentAddress(\"{componentAddress}\") \"withdraw_by_amount\" Decimal(\"{withdrawByAmount}\") ResourceAddress (\"{fromResourceAddress})\"");

        return this;
    }

    public ManifestBuilder WithTakeFromWorktop(string resourceAddress, string bucketName)
    {
        _instructions.Add($"TAKE_FROM_WORKTOP ResourceAddress(\"{resourceAddress}\") Bucket(\"{bucketName}\")");

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

    public ManifestBuilder WithNewAccountWithNonFungibleResource(string publicKey, string bucketName)
    {
        var deriveNonFungibleAddress = "000000000000000000000000000000000000000000000000000002300721000000";

        _instructions.Add(
            $"CALL_FUNCTION PackageAddress(\"GenesisData.AccountPackageAddress\") \"Account\" \"new_with_resource\" Enum(\"Protected\", Enum(\"ProofRule\", Enum(\"Require\", Enum(\"StaticNonFungible\", NonFungibleAddress(\"{deriveNonFungibleAddress}{publicKey}\"))))) Bucket(\"{bucketName}\")");

        return this;
    }
}
