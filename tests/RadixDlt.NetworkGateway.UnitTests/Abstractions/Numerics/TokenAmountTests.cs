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
using RadixDlt.NetworkGateway.Abstractions.Numerics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using Xunit;

namespace RadixDlt.NetworkGateway.UnitTests.Abstractions.Numerics;

public class TokenAmountTests
{
    [Theory]
    [InlineData(false, "123", "", "123")]
    [InlineData(false, "", "12", "0.12")]
    [InlineData(false, "000", "13444", "0.13444")]
    [InlineData(false, "0", "0", "0")]
    [InlineData(false, "11", "000000000000000009", "11.000000000000000009")] // Does hold 18 dp
    [InlineData(false, "11", "0000000000000000009", "11")] // Only holds 18 dp (truncating)
    [InlineData(false, "123.34", "", "NaN")]
    [InlineData(false, "ab", "cd", "NaN")]
    [InlineData(false, "ab", "0", "NaN")]
    [InlineData(true, "1", "0", "-1")]
    [InlineData(true, "-1", "0", "NaN")] // Invalid call
    [InlineData(true, "-1", "-1", "NaN")] // Invalid call
    [InlineData(true, "", "-1", "NaN")] // Invalid call
    [InlineData(false, "12", "-3", "NaN")] // Invalid call
    public void Create_TokenAmountFromStringParts_ReadsCorrectly(bool isNegative, string wholePart, string fractionalPart, string expected)
    {
        var tokenAmount = TokenAmount.FromStringParts(isNegative, wholePart, fractionalPart).ToString();

        tokenAmount.Should().Be(expected);
    }

    [Theory]
    [InlineData("123", "123")]
    [InlineData("0.12", "0.12")]
    [InlineData("000.13444", "0.13444")]
    [InlineData("0.0", "0")]
    [InlineData("11.000000000000000009", "11.000000000000000009")] // Does hold 18 dp
    [InlineData("11.0000000000000000019", "11.000000000000000001")] // Does not hold 19 dp, truncates
    [InlineData("123.34", "123.34")]
    [InlineData("NaN", "NaN")]
    [InlineData("Infinity", "NaN")]
    [InlineData("-Infinity", "NaN")]
    [InlineData("-123", "-123")]
    [InlineData("-123.24", "-123.24")]
    [InlineData("-11.000000000000000009", "-11.000000000000000009")] // Does hold 18 dp
    [InlineData("-11.0000000000000000019", "-11.000000000000000001")] // Does not hold 19 dp, truncates
    [InlineData("-0.0000000000000000019", "-0.000000000000000001")] // Does not hold 19 dp, truncates
    [InlineData("-0.0000000000000000007", "0")] // Does not hold 19 dp, truncates and removes decimal point
    public void Create_FromString_ToStringReadsCorrectly(string decimalString, string expected)
    {
        var tokenAmount = TokenAmount.FromDecimalString(decimalString).ToString();

        tokenAmount.Should().Be(expected);
    }

    [Theory]
    [InlineData("0", "0.000000000000000000")]
    [InlineData("1", "0.000000000000000001")]
    [InlineData("123", "0.000000000000000123")]
    [InlineData("12345678900123456789", "12.345678900123456789")]
    [InlineData("-123", "-0.000000000000000123")]
    [InlineData("-123456789001234567", "-0.123456789001234567")]
    [InlineData("-1234567890012345678", "-1.234567890012345678")]
    public void Create_FromSubUnitsString_ReadsCorrectlyAtFullPrecision(string subUnitsStr, string expected)
    {
        var tokenAmount = TokenAmount.FromSubUnitsString(subUnitsStr).ToStringFullPrecision();

        tokenAmount.Should().Be(expected);
    }

    [Fact]
    public void Equate_NaNWithNaN_ReturnsTrue()
    {
        // This agrees with https://docs.microsoft.com/en-us/dotnet/api/system.double.nan?view=net-5.0
        // (it's just the operators such as == that return false)

        var a = TokenAmount.NaN;
        var b = TokenAmount.NaN;

        a.Should().Be(b);
        b.Should().Be(a);
    }

    [Fact]
    public void Equate_SameTokenAmount_ReturnsTrue()
    {
        var a = TokenAmount.FromDecimalString("123");
        var b = TokenAmount.FromDecimalString("123");

        a.Should().Be(b);
        b.Should().Be(a);
    }

    [Fact]
    public void Equate_DifferentTokenAmount_ReturnsFalse()
    {
        var a = TokenAmount.FromDecimalString("123");
        var b = TokenAmount.FromDecimalString("1234");

        a.Should().NotBe(b);
        b.Should().NotBe(a);
    }

    [Fact]
    public void Equate_NaNWithNoneNaN_ReturnsFalse()
    {
        var a = TokenAmount.FromDecimalString("1");
        var b = TokenAmount.NaN;

        a.Should().NotBe(b);
        b.Should().NotBe(a);
    }

    [Fact]
    public void Equate_SameAmountCreatedTwoDifferentWays_ReturnsTrue()
    {
        var a = TokenAmount.FromDecimalString("123");
        var b = TokenAmount.FromSubUnitsString("123000000000000000000");

        a.Should().Be(b);
        b.Should().Be(a);
    }

    [Fact]
    public void GivenNaN_SubunitsIsZero()
    {
        var act = () => TokenAmount.NaN.GetSubUnits();

        act.Should().Throw<ArithmeticException>();
    }

    public static IEnumerable<object[]> IsNaN_Data => new List<object[]>
    {
        new object[] { TokenAmount.NaN, true },
        new object[] { TokenAmount.FromSubUnits(BigInteger.Zero), false },
        new object[] { TokenAmount.FromSubUnits(BigInteger.One), false },
        new object[] { TokenAmount.FromSubUnits(BigInteger.MinusOne), false },
        new object[] { TokenAmount.FromStringParts(false, "1", "234"), false },
        new object[] { TokenAmount.FromStringParts(false, "-1", "234"), true }, // Invalid call
        new object[] { TokenAmount.FromStringParts(true, "1", "234"), false },
        new object[] { TokenAmount.FromStringParts(false, "1", "-234"), true }, // Invalid call
        new object[] { TokenAmount.FromStringParts(true, "1", "-234"), true }, // Invalid call
        new object[] { TokenAmount.FromDecimalString("5") / TokenAmount.Zero, true },
    };

    [Theory]
    [MemberData(nameof(IsNaN_Data))]
    public void GivenTokenAmount_IsNaN_ReturnsCorrectly(TokenAmount tokenAmount, bool expectedIsNaN)
    {
        tokenAmount.IsNaN().Should().Be(expectedIsNaN);
    }

    [Theory]
    [InlineData("1", "1", "1")]
    [InlineData("100", "10", "10")]
    [InlineData("32", "4", "8")]
    [InlineData("5000000000", "0.2", "25000000000")]
    [InlineData("5000000000", "0.00005", "100000000000000")]
    public void Divide_ExactValue(string dividend, string divisor, string expected)
    {
        var result = TokenAmount.FromDecimalString(dividend) / TokenAmount.FromDecimalString(divisor);

        result.ToString().Should().Be(expected);
    }

    [Theory]
    [InlineData("141", "100", "1.410")]
    [InlineData("100", "141", "0.709")]
    [InlineData("100", "33", "3.030")]
    [InlineData("50000000000", "0.123456789", "405000003685")]
    public void Divide_ApproximateValue(string dividend, string divisor, string expected)
    {
        var result = TokenAmount.FromDecimalString(dividend) / TokenAmount.FromDecimalString(divisor);
        var resultAsNumber = decimal.Parse(result.ToString(), NumberFormatInfo.InvariantInfo);
        var expectedAsNumber = decimal.Parse(expected, NumberFormatInfo.InvariantInfo);

        resultAsNumber.Should().BeApproximately(expectedAsNumber, 100);
    }

    [Theory]
    [InlineData("12.5", "2", "25")]
    [InlineData("2", "5", "10")]
    [InlineData("2.1", "2", "4.2")]
    [InlineData("50000000000", "0.123456789", "6172839450")]
    public void Multiply_ExactValue(string op1, string op2, string expected)
    {
        var result = TokenAmount.FromDecimalString(op1) * TokenAmount.FromDecimalString(op2);
        var resultAsNumber = decimal.Parse(result.ToString(), NumberFormatInfo.InvariantInfo);
        var expectedAsNumber = decimal.Parse(expected, NumberFormatInfo.InvariantInfo);

        resultAsNumber.Should().Be(expectedAsNumber);
    }
}
