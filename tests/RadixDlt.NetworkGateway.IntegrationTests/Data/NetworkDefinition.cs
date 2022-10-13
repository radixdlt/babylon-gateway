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

using System;
using System.Text;

namespace RadixDlt.NetworkGateway.IntegrationTests.Data;

public enum NetworkEnum
{
    Adapanet = 10, // Babylon Alphanet
    Enkinet = 33,
    Localnet = 240,
    IntegrationTests = 241,
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
        sb.Append("  Id: ").Append(Id).Append("\n");
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
