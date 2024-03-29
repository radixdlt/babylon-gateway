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
using RadixDlt.NetworkGateway.Abstractions.Extensions;
using RadixDlt.NetworkGateway.Abstractions.Network;
using System.Collections.Generic;
using Xunit;

namespace RadixDlt.NetworkGateway.UnitTests.Abstractions.Network;

public class RadixAddressTests
{
    [Theory]
    [InlineData("account_loc1qdj0c2rrk5v9yv87gr9zz9mdcrjpxq60fyw0n6j57q3szeyglx")]
    [InlineData("resource_loc1qrky4vgxu4pqpk07k6hem2kx23wshq4renre04smrlwsvvyu3v")]
    public void WhenGiven_EncodedStringWithValidRadixAddress_DecodeAndReencodeIsIdentity(string encodedString)
    {
        var decodedData = RadixAddressCodec.Decode(encodedString);
        var reEncodedString = RadixAddressCodec.Encode(decodedData.Hrp, decodedData.Data);

        decodedData.Variant.Should().Be(Bech32Codec.Variant.Bech32M);
        reEncodedString.Should().Be(encodedString.ToLowerInvariant());
    }

    [Theory]
    [InlineData("account_loc1qdj0c2rrk5v9yv87gr9zz9mdcrjpxq60fyw0n6j57q3szeyglx", "0364fc2863b5185230fe40ca21176dc0e413034f491cf9ea54f023")]
    [InlineData("resource_loc1qrky4vgxu4pqpk07k6hem2kx23wshq4renre04smrlwsvvyu3v", "00ec4ab106e54200d9feb6af9daac6545d0b82a3ccc797d61b1fdd")]
    public void WhenGiven_EncodedStringWithValidRadixAddress_DecodeGivesAddress(string encodedString, string expectedAddress)
    {
        var decodedData = RadixAddressCodec.Decode(encodedString);
        var decodedHex = decodedData.Data.ToHex();

        decodedHex.Should().Be(expectedAddress);
    }

    public static IEnumerable<object[]> Invalid_Bech32Strings => new List<object[]>
    {
        new object[] { (char)0x20 + "1nwldj5" }, // HRP character out of range
        new object[] { (char)0x7F + "1axkwrx" }, // HRP character out of range
        new object[] { (char)0x80 + "1eym55h" }, // HRP character out of range
        new object[] { "de1lg7wt" + (char)0xFF }, // Invalid character in checksum
        new object[] { "an84characterslonghumanreadablepartthatcontainsthenumber1andtheexcludedcharactersbio1569pvx" }, // HRP character out of range
        new object[] { "pzry9x0s0muk" }, // No separator character
        new object[] { "1pzry9x0s0muk" }, // Empty HRP
        new object[] { "x1b4n0q5v" }, // Invalid data character
        new object[] { "li1dgmt3" }, // Too short checksum
        new object[] { "A1G7SGD8" }, // checksum calculated with uppercase form of HRP
        new object[] { "10a06t8" }, // empty HRP
        new object[] { "vb1qvx0emaq0tua6md7wu9c047mm5krrwnlfl8c7ws3jm2s9uf4vxcyvrwrazz" }, // Bad checksum
    };

    [Theory]
    [MemberData(nameof(Invalid_Bech32Strings))]
    public void WhenGiven_InvalidEncodedString_DecodeThrows(string encodedString)
    {
        var act = () => RadixAddressCodec.Decode(encodedString);

        act.Should().Throw<AddressException>();
    }
}
