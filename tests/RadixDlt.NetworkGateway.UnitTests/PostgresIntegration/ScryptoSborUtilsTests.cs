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

using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using RadixDlt.NetworkGateway.PostgresIntegration;
using System;
using System.Collections.Generic;
using Xunit;
using GatewayModel = RadixDlt.NetworkGateway.GatewayApiSdk.Model;

namespace RadixDlt.NetworkGateway.UnitTests.PostgresIntegration;

public class ScryptoSborUtilsTests
{
    [Fact]
    public void ParseStringScalarMetadata()
    {
        const string Input = "5c2200010c124d79206d6574616461746120737472696e67";
        var result = ScryptoSborUtils.DecodeToGatewayMetadataItemValue(Convert.FromHexString(Input), 1);

        result.ShouldNotBeNull();
        result.Should().BeOfType<GatewayModel.MetadataStringValue>();
        (result as GatewayModel.MetadataStringValue)!.Value.Should().BeEquivalentTo("My metadata string");
    }

    [Fact]
    public void ParseStringArrayMetadata()
    {
        const string Input = "5c228001200c021a4d79206d6574616461746120617272617920737472696e6720311a4d79206d6574616461746120617272617920737472696e672032";
        var result = ScryptoSborUtils.DecodeToGatewayMetadataItemValue(Convert.FromHexString(Input), 1);
        var expected = new List<string> { "My metadata array string 1", "My metadata array string 2" };

        result.ShouldNotBeNull();
        result.Should().BeOfType<GatewayModel.MetadataStringArrayValue>();
        (result as GatewayModel.MetadataStringArrayValue)!.Values.Should().BeEquivalentTo(expected);
    }

    [Theory]
    [InlineData("5c2200010c0548656c6c6f")]
    [InlineData("5c228001200c020548656c6c6f06776f726c6421")]
    [InlineData("5c2201010101")]
    [InlineData("5c2281012001020100")]
    [InlineData("5c2202010701")]
    [InlineData("5c2282012007020102")]
    [InlineData("5c2203010902000000")]
    [InlineData("5c2283012009020200000003000000")]
    [InlineData("5c2204010a0300000000000000")]
    [InlineData("5c228401200a0203000000000000000400000000000000")]
    [InlineData("5c2205010404000000")]
    [InlineData("5c2285012004020400000005000000")]
    [InlineData("5c220601050500000000000000")]
    [InlineData("5c22860120050205000000000000000600000000000000")]
    [InlineData("5c220701a0000064a7b3b6e00d00000000000000000000000000000000")]
    [InlineData("5c22870120a002000064a7b3b6e00d000000000000000000000000000000000000000000000000000052acdfb2241d0000000000000000")]
    [InlineData("5c220801805da66318c6318c61f5a61b4c6318c6318cf794aa8d295f14e6318c6318c6")]
    [InlineData("5c2288012080015da66318c6318c61f5a61b4c6318c6318cf794aa8d295f14e6318c6318c6")]
    [InlineData("5c2209012201012007200000000000000000000000000000000000000000000000000000000000000000")]
    [InlineData("5c228901202202010120072000000000000000000000000000000000000000000000000000000000000000000001200721000000000000000000000000000000000000000000000000000000000000000000")]
    [InlineData("5c220a012102809a4c6318c6318c60db1ff8cc6318c6318cf7c75456aba2fbc6318c6318c6c0022043633bb90fe8ed9c006d718d57e51b644519f36fa9cf033bb83d72d77247a5ec")]
    [InlineData("5c228a0120210102809a4c6318c6318c60db1ff8cc6318c6318cf7c75456aba2fbc6318c6318c6c0022043633bb90fe8ed9c006d718d57e51b644519f36fa9cf033bb83d72d77247a5ec")]
    [InlineData("5c220b01c0000b48656c6c6f5f776f726c64")]
    [InlineData("5c228b0120c003000b48656c6c6f5f776f726c6401000000000000002a020101")]
    [InlineData("5c220c01057962946400000000")]
    [InlineData("5c228c012005017962946400000000")]
    [InlineData("5c220d010c1868747470733a2f2f7777772e7261646978646c742e636f6d")]
    [InlineData("5c228d01200c011868747470733a2f2f7777772e7261646978646c742e636f6d")]
    [InlineData("5c220e010c107777772e7261646978646c742e636f6d")]
    [InlineData("5c228e01200c01107777772e7261646978646c742e636f6d")]
    [InlineData("5c220f0122010120071d6a8a691dae2cd15ed0369931ce0a949ecafa5c3f93f8121833646e15c3")]
    [InlineData("5c228f01202202010120071d6a8a691dae2cd15ed0369931ce0a949ecafa5c3f93f8121833646e15c3000120071d165dee785924e7421a0fd0418a19d5daeec395fd505a92a0fd3117e428")]
    public void ShouldNotFail(string input)
    {
        var result = ScryptoSborUtils.DecodeToGatewayMetadataItemValue(Convert.FromHexString(input), 1);

        result.ShouldNotBeNull();
    }
}
