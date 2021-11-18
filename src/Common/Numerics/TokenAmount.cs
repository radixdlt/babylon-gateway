using Common.Extensions;
using System.Numerics;

namespace Common.Numerics;

public readonly record struct TokenAmount
{
    public static readonly TokenAmount Zero = new(0);
    public static readonly TokenAmount NaN = new(true);
    public static readonly string StringForNan = "NaN";

    private const int DecimalPrecision = 18;
    private const int MaxPostgresPrecision = 1000;

    // We under-estimate this so that we're definitely safe!
    private static readonly int _safeByteLengthLimitBeforePostgresError = (int)Math.Floor(MaxPostgresPrecision * Math.Log(10, 256));
    private static readonly BigInteger _divisor = BigInteger.Pow(10, DecimalPrecision);

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
    public static TokenAmount operator -(TokenAmount a) => new(-a._subUnits);
    public static TokenAmount operator +(TokenAmount a, TokenAmount b) => new(a._subUnits + b._subUnits);
    public static TokenAmount operator -(TokenAmount a, TokenAmount b) => new(a._subUnits - b._subUnits);

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

    /// <summary>
    /// Calculates the corresponding string for postgres handling, assuming decimal(1000, 18).
    /// If the number is too large, we return NaN so that we can still ingest the transaction.
    /// See <a href="https://www.postgresql.org/docs/14/datatype-numeric.html">the Postgres Numeric docs</a>.
    ///
    /// Unfortunately, NPGSQL is yet to support NaN in their BigInteger numeric fields.
    /// </summary>
    public string ToPostgresDecimal()
    {
        return _isNaN || IsUnsafeSizeForPostgres() ? StringForNan : ToString();
    }

    public string ToSubUnitString()
    {
        return _isNaN ? StringForNan : _subUnits.ToString();
    }

    public override string ToString()
    {
        if (_isNaN)
        {
            return StringForNan;
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

    public BigInteger GetSubUnitsSafeForPostgres()
    {
        if (_isNaN)
        {
            throw new ArithmeticException("TokenAmount is NaN, cannot get SubUnits.");
        }

        if (IsUnsafeSizeForPostgres())
        {
            throw new ArithmeticException("TokenAmount is too large to persist to PostGres.");
        }

        return _subUnits;
    }

    /// <summary>
    /// Calculates whether the string is certainly safe for postgres handling, assuming decimal(1000, 18).
    /// </summary>
    private bool IsUnsafeSizeForPostgres()
    {
        return _subUnits.GetByteCount(isUnsigned: false) >= _safeByteLengthLimitBeforePostgresError;
    }
}
