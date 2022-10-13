using System;
using System.Text;

namespace RadixDlt.NetworkGateway.IntegrationTests.Data;

public enum NetworkEnum
{
    Localnet = 0,
    IntegrationTests = 1,
    Enkinet = 2,
    Adapanet = 3, // Babylon Alphanet
}

public class NetworkDefinition
{
    public NetworkDefinition(int id, string logicalName, string hrpSuffix)
    {
        if (id <= 0 || id > 255)
        {
            throw new ArgumentException(
                "Id should be between 1 and 255 so it isn't default(int) = 0 and will fit into a byte if we change in future");
        }

        Id = id;
        LogicalName = logicalName;
        HrpSuffix = hrpSuffix;
        PackageHrp = "package_" + hrpSuffix;
        NormalComponentHrp = "component_" + hrpSuffix;
        AccountComponentHrp = "account_" + hrpSuffix;
        SystemComponentHrp = "system_" + hrpSuffix;
        ValidatorHrp = "validator_" + hrpSuffix;
        ResourceHrp = "resource_" + hrpSuffix;
        NodeHrp = "node_" + hrpSuffix;
    }

    public int Id { get; }

    public string PackageHrp { get; }

    public string NormalComponentHrp { get; }

    public string AccountComponentHrp { get; }

    public string SystemComponentHrp { get; }

    public string ValidatorHrp { get; }

    public string ResourceHrp { get; }

    public string NodeHrp { get; }

    public string LogicalName { get; }

    public string HrpSuffix { get; }

    public static NetworkDefinition Get(NetworkEnum network)
    {
        switch (network)
        {
            case NetworkEnum.Enkinet:
                return new NetworkDefinition(33 /* 0x21 */, "enkinet", "tdx_20_");

            case NetworkEnum.Localnet:
                return new NetworkDefinition(240 /* 0xF0 */, "localnet", "loc_");

            case NetworkEnum.IntegrationTests:
                return new NetworkDefinition(241 /* 0xF1 */, "inttestnet", "test_");

            case NetworkEnum.Adapanet:
                return new NetworkDefinition(10 /* 0x0a */, "adapanet", "tdx_a_");

            default:
                throw new NotImplementedException();
        }
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("Network Definition:\n");
        sb.Append("  LogicalName: ").Append(LogicalName).Append("\n");
        sb.Append("  HrpSuffix: ").Append(HrpSuffix).Append("\n");
        // sb.Append("  PackageHrp: ").Append(PackageHrp).Append("\n");
        // sb.Append("  NormalComponentHrp: ").Append(NormalComponentHrp).Append("\n");
        // sb.Append("  AccountComponentHrp: ").Append(AccountComponentHrp).Append("\n");
        // sb.Append("  SystemComponentHrp: ").Append(SystemComponentHrp).Append("\n");
        // sb.Append("  ValidatorHrp: ").Append(ValidatorHrp).Append("\n");
        // sb.Append("  ResourceHrp: ").Append(ResourceHrp).Append("\n");
        // sb.Append("  NodeHrp: ").Append(NodeHrp).Append("\n");
        return sb.ToString();
    }
}
