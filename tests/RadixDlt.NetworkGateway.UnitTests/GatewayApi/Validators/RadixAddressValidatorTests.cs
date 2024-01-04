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
using Moq;
using RadixDlt.NetworkGateway.Abstractions.Addressing;
using RadixDlt.NetworkGateway.GatewayApi.Validators;
using System.Threading;
using Xunit;

namespace RadixDlt.NetworkGateway.UnitTests.GatewayApi.Validators;

public class RadixAddressValidatorTests
{
    [Theory]
    // Mainnet.
    [InlineData("consensusmanager_rdx1scxxxxxxxxxxcnsmgrxxxxxxxxx000999665565xxxxxxxxxcnsmgr", "rdx")]
    [InlineData("resource_rdx1tknxxxxxxxxxradxrdxxxxxxxxx009923554798xxxxxxxxxradxrd", "rdx")]
    [InlineData("resource_rdx1nfxxxxxxxxxxsystxnxxxxxxxxx002683325037xxxxxxxxxsystxn", "rdx")]
    [InlineData("component_rdx1cptxxxxxxxxxfaucetxxxxxxxxx000527798379xxxxxxxxxfaucet", "rdx")]
    [InlineData("internal_component_rdx1lrqk2ksa56frjv5hcwplkkfydek52pajryqeut58v2fzvchjdsv2t5", "rdx")]
    [InlineData("account_rdx16y80c39tqcsry0hejwp2wtsgzn3q3u0la27vvn9tjs309c4ppzzh4v", "rdx")]
    [InlineData("package_rdx1pkgxxxxxxxxxresrcexxxxxxxxx000538436477xxxxxxxxxresrce", "rdx")]
    [InlineData("internal_keyvaluestore_rdx1krqvh6a49s57c7rg0n9xvdaqq74342wj6jvta0y7kzkrfcwshefpzx", "rdx")]
    [InlineData("internal_vault_rdx1tz3pl56e29v3yz7gps0vtl77m8yuj0zrclfthr95uwe35fz7v0d68t", "rdx")]
    [InlineData("internal_vault_rdx1nruptzlvyyjpch28dzfrtpky0z8sywqcs9z2vz9x00rvsmlevzvt30", "rdx")]
    [InlineData("validator_rdx1sv2nu2y6wmhcg4d99mjek5g8qmpc2ua73yfaz6tytgrasftamn9c2u", "rdx")]
    [InlineData("accesscontroller_rdx1cva6mtja4crwxxhmd63q2xlhew7fh0af67zw3snhzj8cm7xq2cm06g", "rdx")]
    [InlineData("pool_rdx1c325zs6dz3un8ykkjavy9fkvvyzarkaehgsl408qup6f95aup3le3w", "rdx")]
    [InlineData("pool_rdx1c5s3l6r7r53395ervsplly28e02sfyew80afdrlm88mm5egcajqnav", "rdx")]
    [InlineData("pool_rdx1ccya22mahz5rf0408vxrn59p76f603y85wjhwtaqzr5yp3we3spv8k", "rdx")]
    [InlineData("transactiontracker_rdx1stxxxxxxxxxxtxtrakxxxxxxxxx006844685494xxxxxxxxxtxtrak", "rdx")]
    // Stokenet
    [InlineData("consensusmanager_tdx_2_1scxxxxxxxxxxcnsmgrxxxxxxxxx000999665565xxxxxxxxxv6cg29", "tdx_2_")]
    [InlineData("resource_tdx_2_1tknxxxxxxxxxradxrdxxxxxxxxx009923554798xxxxxxxxxtfd2jc", "tdx_2_")]
    [InlineData("resource_tdx_2_1nfxxxxxxxxxxsystxnxxxxxxxxx002683325037xxxxxxxxxcss8hx", "tdx_2_")]
    [InlineData("component_tdx_2_1cptxxxxxxxxxfaucetxxxxxxxxx000527798379xxxxxxxxxyulkzl", "tdx_2_")]
    [InlineData("internal_component_tdx_2_1lz7r4e30g7gdncc54zxppynqvnkaqfsj46lc8xjzqedqetvtwp0jkq", "tdx_2_")]
    [InlineData("account_tdx_2_168e8u653alt59xm8ple6khu6cgce9cfx9mlza6wxf7qs3wwdyqvusn", "tdx_2_")]
    [InlineData("package_tdx_2_1phua8spmaxapwq56stduucrvztk92gxzjy9c98h0qemfjec0fuqeng", "tdx_2_")]
    [InlineData("internal_keyvaluestore_tdx_2_1kpkxjgh28rp2e2fudwfx3ck9sau86xzt3ckc7gnl25rd6dlta2xfsr", "tdx_2_")]
    [InlineData("internal_vault_tdx_2_1tpxzhhq9kxkxlaken2q7l7q977f2fnv62fexklnwwsw8yp4vlenzg2", "tdx_2_")]
    [InlineData("internal_vault_tdx_2_1nqhwlrwsq3g99n5kxnly9j58ase6ncwe0ce80dyq92f233jcyphmv2", "tdx_2_")]
    [InlineData("validator_tdx_2_1s086l0qqxqel2c0mxu9kspqs0ccrkytskkzus2sqscdl882qh0l7xy", "tdx_2_")]
    [InlineData("accesscontroller_tdx_2_1cdl6vnxuw73k5dhzs366yqse6wey62dcvh25qshyxhq7g0ev2m9fvc", "tdx_2_")]
    [InlineData("identity_tdx_2_1c24z6a33fevvs9v02dla0glwekqlfzaevtw4mf2fft85ptgxdchpea", "tdx_2_")]
    [InlineData("pool_tdx_2_1cjqxg4eflklag38dndehgk8qgzxal5dq6gkneqz9rzfxgln0aepmgm", "tdx_2_")]
    [InlineData("pool_tdx_2_1c5mplf9rxrht4rm9pq2dx3euqh4glccgfq6wldynl6t4ryjzg680pe", "tdx_2_")]
    [InlineData("pool_tdx_2_1ce7l2agqxjqpzydzrff9dm54pp4x56jmdh3a59pn4nljm5u4thclqt", "tdx_2_")]
    [InlineData("transactiontracker_tdx_2_1stxxxxxxxxxxtxtrakxxxxxxxxx006844685494xxxxxxxxxxzw7jp", "tdx_2_")]
    // Mardunet.
    [InlineData("package_tdx_24_1pkgxxxxxxxxxpackgexxxxxxxxx000726633226xxxxxxxxxzg3awl", "tdx_24_")]
    [InlineData("consensusmanager_tdx_24_1scxxxxxxxxxxcnsmgrxxxxxxxxx000999665565xxxxxxxxxppddvz", "tdx_24_")]
    [InlineData("component_tdx_24_1cptxxxxxxxxxgenssxxxxxxxxxx000977302539xxxxxxxxx5s96fx", "tdx_24_")]
    [InlineData("transactiontracker_tdx_24_1stxxxxxxxxxxtxtrakxxxxxxxxx006844685494xxxxxxxxxkvyj6g", "tdx_24_")]
    [InlineData("resource_tdx_24_1tknxxxxxxxxxradxrdxxxxxxxxx009923554798xxxxxxxxxd3jrej", "tdx_24_")]
    [InlineData("internal_vault_tdx_24_1tz3pl56e29v3yz7gps0vtl77m8yuj0zrclfthr95uwe35fz7thnrr8", "tdx_24_")]
    [InlineData("internal_keyvaluestore_tdx_24_1krn7clzr3qmq2zhwr77mdenksxswf00yeh8tn3vyzesg4kr3rru8r5", "tdx_24_")]
    [InlineData("account_tdx_24_168e8u653alt59xm8ple6khu6cgce9cfx9mlza6wxf7qs3wwd6f23gn", "tdx_24_")]
    [InlineData("validator_tdx_24_1sdtnujyn3720ymg8lakydkvc5tw4q3zecdj95akdwt9de362lyycpp", "tdx_24_")]
    [InlineData("pool_tdx_24_1c432yjjqvlx0qkyyrnsmdz6v48gt5ve5rwvzlue2mmqrwd0wlp5537", "tdx_24_")]
    [InlineData("resource_tdx_24_1n2xjpv8mzg9uylnwzqnkezv3w5guq4psrcvstr3q3ylkaclzanwyq5", "tdx_24_")]
    [InlineData("pool_tdx_24_1cegjsuzf8rkz362dzt63wgmvglyy77u9uhaa0e872tc7lk7znhyp2h", "tdx_24_")]
    [InlineData("identity_tdx_24_1c2gr9p90x4009jzsqmzjv2hcgru8lzslg43cmswxk9ta62ffxzha2k", "tdx_24_")]
    [InlineData("internal_component_tdx_24_1lpm0l0egtyckkux2hf4r49tqekc3tztzawqvm57pw2t3r7yh4qzl5h", "tdx_24_")]
    [InlineData("internal_keyvaluestore_tdx_24_1kpkxjgh28rp2e2fudwfx3ck9sau86xzt3ckc7gnl25rd6dltc023jk", "tdx_24_")]
    [InlineData("internal_vault_tdx_24_1nqhwlrwsq3g99n5kxnly9j58ase6ncwe0ce80dyq92f233jcemy2t8", "tdx_24_")]
    [InlineData("accesscontroller_tdx_24_1cd9e0x20w8e6fh5zcyv0rzzpz94n275ldxs26w868x5davl7s2w47v", "tdx_24_")]
    public void WhenGiven_ValidValue_Succeeds(string address, string expectedNetworkHrpSuffix)
    {
        address.Should().NotBe(expectedNetworkHrpSuffix);
        // TODO restore
        // // Prepare.
        // var mockNetworkConfigurationProvider = new Mock<INetworkConfigurationProvider>();
        // mockNetworkConfigurationProvider.Setup(x => x.GetNetworkConfiguration(It.IsAny<CancellationToken>())).ReturnsAsync(new NetworkConfiguration(36, "mardunet", 1, 1, null!, new HrpDefinition(Suffix: expectedNetworkHrpSuffix)));
        // var validator = new RadixAddressValidator(mockNetworkConfigurationProvider.Object);
        //
        // // Act.
        // var result = validator.Validate(address);
        //
        // // Assert.
        // result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("a", "rdx")]
    [InlineData("abc", "rdx")]
    [InlineData("account_loc1q0w8pk0vlwt75v4dhxrcvpzl3r2vzqkvwdwffzzu69zqvep9l2", "rdx")] // single invalid character -> invalid checksum
    [InlineData("resource_rdx1tknxxxxxxxxxradxrdxxxxxxxxx009923554798xxxxxxxxxradxrd", "tdx_2_")]
    [InlineData("resource_tdx_24_1tknxxxxxxxxxradxrdxxxxxxxxx009923554798xxxxxxxxxd3jrej", "rdx")]
    [InlineData("consensusmanager_tdx_2_1scxxxxxxxxxxcnsmgrxxxxxxxxx000999665565xxxxxxxxxv6cg29", "tdx_24_")]
    public void WhenGiven_InvalidValue_Fails(string address, string expectedNetworkHrpSuffix)
    {
        address.Should().NotBe(expectedNetworkHrpSuffix);
        // TODO restore
        // // Prepare.
        // var mockNetworkConfigurationProvider = new Mock<INetworkConfigurationProvider>();
        // mockNetworkConfigurationProvider.Setup(x => x.GetNetworkHrpSuffix()).Returns(expectedNetworkHrpSuffix);
        // var validator = new RadixAddressValidator(mockNetworkConfigurationProvider.Object);
        //
        // // Act.
        // var result = validator.Validate(address);
        //
        // // Assert.
        // result.IsValid.Should().BeFalse();
    }
}
