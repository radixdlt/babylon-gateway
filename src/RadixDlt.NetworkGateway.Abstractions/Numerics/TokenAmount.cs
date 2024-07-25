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

using RadixDlt.NetworkGateway.Abstractions.Extensions;
using System;
using System.Numerics;

namespace RadixDlt.NetworkGateway.Abstractions.Numerics;

public readonly record struct TokenAmount : IComparable<TokenAmount>
{
    public const string StringForNaN = "NaN";

    public static readonly TokenAmount Zero;
    public static readonly TokenAmount NaN;
    public static readonly TokenAmount OneFullUnit;

    private const int DecimalPrecision = 18;

    private static readonly BigInteger _divisor;

    static TokenAmount()
    {
        _divisor = BigInteger.Pow(10, DecimalPrecision);

        Zero = new TokenAmount(0);
        NaN = new TokenAmount(true);
        OneFullUnit = new TokenAmount(_divisor);
    }

    private readonly BigInteger _subUnits;
    private readonly bool _isNaN;

    public static TokenAmount FromSubUnits(BigInteger subUnits)
    {
        return new TokenAmount(subUnits);
    }

    public static TokenAmount FromSubUnitsString(string subUnitsString)
    {
        return BigInteger.TryParse(subUnitsString, out var subUnits)
            ? new TokenAmount(subUnits)
            : NaN;
    }

    public static TokenAmount FromStringParts(bool isNegative, string wholePart, string decimalPart)
    {
        var wholeUnitsString = string.IsNullOrEmpty(wholePart) ? "0" : wholePart;
        var subUnitsString = string.IsNullOrEmpty(decimalPart)
            ? "0"
            : decimalPart.Truncate(DecimalPrecision).PadRight(DecimalPrecision, '0');
        var multiplier = isNegative ? BigInteger.MinusOne : BigInteger.One;
        return (
            BigInteger.TryParse(wholeUnitsString, out var wholeUnits) &&
            wholeUnits >= 0 &&
            BigInteger.TryParse(subUnitsString, out var subUnits) &&
            subUnits >= 0
        )
            ? new TokenAmount(multiplier * wholeUnits, multiplier * subUnits)
            : NaN;
    }

    public static TokenAmount FromDecimalString(string decimalString)
    {
        if (decimalString == "NaN")
        {
            return NaN;
        }

        if (string.IsNullOrEmpty(decimalString))
        {
            return NaN;
        }

        var isNegative = decimalString.StartsWith("-");
        var nonNegativeDecimalString = isNegative ? decimalString[1..] : decimalString;

        if (!decimalString.Contains('.'))
        {
            return FromStringParts(isNegative, nonNegativeDecimalString, string.Empty);
        }

        return nonNegativeDecimalString.Split('.').TryInterpretAsPair(out var wholePart, out var fractionalPart)
            ? FromStringParts(isNegative, wholePart, fractionalPart)
            : NaN;
    }

    public static TokenAmount operator +(TokenAmount a) => a;

    public static TokenAmount operator -(TokenAmount a) => a.IsNaN() ? NaN : new TokenAmount(-a._subUnits);

    public static TokenAmount operator +(TokenAmount a, TokenAmount b) => (a.IsNaN() || b.IsNaN()) ? NaN : new TokenAmount(a._subUnits + b._subUnits);

    public static TokenAmount operator -(TokenAmount a, TokenAmount b) => (a.IsNaN() || b.IsNaN()) ? NaN : new TokenAmount(a._subUnits - b._subUnits);

    public static TokenAmount operator *(TokenAmount a, TokenAmount b) => (a.IsNaN() || b.IsNaN()) ? NaN : new TokenAmount(a._subUnits * b._subUnits);

    public static TokenAmount operator /(TokenAmount a, TokenAmount b) => (a.IsNaN() || b.IsNaN()) ? NaN : Divide(a, b);

    // ReSharper disable SimplifyConditionalTernaryExpression - As it's clearer as written
#pragma warning disable IDE0075
    public static bool operator <(TokenAmount a, TokenAmount b) => (a.IsNaN() || b.IsNaN()) ? false : a._subUnits < b._subUnits;

    public static bool operator >(TokenAmount a, TokenAmount b) => (a.IsNaN() || b.IsNaN()) ? false : a._subUnits > b._subUnits;

    public static bool operator <=(TokenAmount a, TokenAmount b) => (a.IsNaN() || b.IsNaN()) ? false : a._subUnits <= b._subUnits;

    public static bool operator >=(TokenAmount a, TokenAmount b) => (a.IsNaN() || b.IsNaN()) ? false : a._subUnits >= b._subUnits;
#pragma warning restore IDE0075
    // ReSharper restore SimplifyConditionalTernaryExpression

    public static TokenAmount Min(TokenAmount a, TokenAmount b) => (a.IsNaN() || b.IsNaN()) ? NaN : a <= b ? a : b;

    public static TokenAmount Max(TokenAmount a, TokenAmount b) => (a.IsNaN() || b.IsNaN()) ? NaN : a >= b ? a : b;

    private TokenAmount(BigInteger subUnits)
    {
        _subUnits = subUnits;
        _isNaN = false;
    }

    private TokenAmount(BigInteger wholeUnits, BigInteger subUnits)
    {
        _subUnits = (wholeUnits * _divisor) + subUnits;
        _isNaN = false;
    }

    private TokenAmount(bool isNaN)
    {
        _subUnits = BigInteger.Zero;
        _isNaN = isNaN;
    }

    public string ToSubUnitString()
    {
        return _isNaN ? StringForNaN : _subUnits.ToString();
    }

    public override string ToString()
    {
        if (_isNaN)
        {
            return StringForNaN;
        }

        var (isNegative, integerPart, fractionalPart) = GetIntegerAndFractionalParts();
        return fractionalPart == 0
            ? $"{(isNegative ? "-" : string.Empty)}{integerPart}"
            : $"{(isNegative ? "-" : string.Empty)}{integerPart}.{fractionalPart.ToString().PadLeft(DecimalPrecision, '0').TrimEnd('0')}";
    }

    public string ToStringFullPrecision()
    {
        if (IsNaN())
        {
            return "NaN";
        }

        var (isNegative, integerPart, fractionalPart) = GetIntegerAndFractionalParts();
        return $"{(isNegative ? "-" : string.Empty)}{integerPart}.{fractionalPart.ToString().PadLeft(DecimalPrecision, '0')}";
    }

    public (bool IsNegative, BigInteger IntegerPart, BigInteger FractionalPart) GetIntegerAndFractionalParts()
    {
        // This rounds towards 0 and can outputs a negative fractional part
        var integerPart = BigInteger.DivRem(_subUnits, _divisor, out var fractionalPart);

        if (integerPart < 0 || fractionalPart < 0)
        {
            return (true, -integerPart, -fractionalPart);
        }

        return (false, integerPart, fractionalPart);
    }

    public bool IsPositive()
    {
        return GetSubUnits() > 0;
    }

    public bool IsZero()
    {
        return GetSubUnits() == 0;
    }

    public bool IsNegative()
    {
        return GetSubUnits() < 0;
    }

    public bool IsNaN()
    {
        return _isNaN;
    }

    public BigInteger GetSubUnits()
    {
        if (_isNaN)
        {
            throw new ArithmeticException("TokenAmount is NaN, cannot get SubUnits.");
        }

        return _subUnits;
    }

    public int CompareTo(TokenAmount other)
    {
        var isNaNComparison = _isNaN.CompareTo(other._isNaN);
        return isNaNComparison != 0 ? isNaNComparison : _subUnits.CompareTo(other._subUnits);
    }

    // Heavily inspired by https://www.codeproject.com/Articles/5366079/BigDecimal-in-Csharp
    // Licensed under CPOL: https://en.wikipedia.org/wiki/Code_Project_Open_License
    // Author: Paulo Francisco Zemek. August, 01, 2023.
    private static TokenAmount Divide(TokenAmount dividend, TokenAmount divisor)
    {
        if (divisor == Zero)
        {
            // This rule might look odd, but when simplifying expressions, x/x (x divided by x) is 1.
            // So, to keep the rule true, 0 divided by 0 is also 1.
            if (dividend == Zero)
            {
                return OneFullUnit;
            }

            throw new DivideByZeroException($"{nameof(divisor)} can only be zero if {nameof(dividend)} is zero.");
        }

        var doublePrecisionDividendSubUnits = dividend._subUnits * _divisor;
        var divisorSubUnits = divisor._subUnits;
        var doublePrecisionResult = doublePrecisionDividendSubUnits / divisorSubUnits;

        var decimalIndexFromEnd = DecimalPrecision;
        var digitCount = CountDigits(doublePrecisionResult);

        while (decimalIndexFromEnd > 0 && (doublePrecisionResult % 10) == 0)
        {
            decimalIndexFromEnd--;
            doublePrecisionResult /= 10;
            digitCount--;
        }

        // TODO this is wrong! we must
        return FromSubUnits(doublePrecisionResult);
    }

    // Is there a faster approach that works with BigIntegers?
    // It seems Log10 isn't faster at all.
    private static int CountDigits(BigInteger value)
    {
        int count = 0;

        while (value > 0)
        {
            count++;
            value /= 10;
        }

        return count;
    }
}
