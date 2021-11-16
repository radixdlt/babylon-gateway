using Common.Extensions;
using System.Numerics;

namespace Common.Numerics;

public readonly record struct TokenAmount
{
    public static readonly TokenAmount Zero = new(0);
    public static readonly TokenAmount NaN = new(true);

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

    public static TokenAmount FromSubUnits(string subUnitsString)
    {
        return BigInteger.TryParse(subUnitsString, out var subUnits)
            ? new TokenAmount(subUnits)
            : NaN;
    }

    public static TokenAmount FromStringParts(string wholePart, string decimalPart)
    {
        var wholeUnitsString = string.IsNullOrEmpty(wholePart) ? "0" : wholePart;
        var subUnitsString = string.IsNullOrEmpty(decimalPart)
            ? "0"
            : decimalPart.Truncate(DecimalPrecision).PadRight(DecimalPrecision, '0');
        return (
            BigInteger.TryParse(wholeUnitsString, out var wholeUnits) &&
            BigInteger.TryParse(subUnitsString, out var subUnits) &&
            subUnits >= 0
        )
            ? new TokenAmount(wholeUnits, subUnits)
            : NaN;
    }

    public static TokenAmount FromDecimalString(string decimalString)
    {
        if (decimalString == "NaN")
        {
            return NaN;
        }

        if (!decimalString.Contains('.'))
        {
            return FromStringParts(decimalString, string.Empty);
        }

        return decimalString.Split('.').TryInterpretAsPair(out var wholePart, out var fractionalPart)
            ? FromStringParts(wholePart, fractionalPart)
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
    /// </summary>
    public string ToPostgresDecimal()
    {
        return _subUnits.GetByteCount(isUnsigned: true) >= _safeByteLengthLimitBeforePostgresError ? "NaN" : ToString();
    }

    public string ToSubUnitString()
    {
        return IsNaN() ? "NaN" : _subUnits.ToString();
    }

    public override string ToString()
    {
        if (IsNaN())
        {
            return "NaN";
        }

        var integerPart = _subUnits / _divisor;
        var fractionalPart = _subUnits - (integerPart * _divisor);
        return fractionalPart == 0
            ? integerPart.ToString()
            : $@"{integerPart}.{fractionalPart.ToString().PadLeft(DecimalPrecision, '0').TrimEnd('0')}";
    }

    public string ToStringFullPrecision()
    {
        if (IsNaN())
        {
            return "NaN";
        }

        var (integerPart, fractionalPart) = GetIntegerAndFractionalParts();
        return $@"{integerPart}.{fractionalPart.ToString().PadLeft(DecimalPrecision, '0')}";
    }

    public (BigInteger IntegerPart, BigInteger FractionalPart) GetIntegerAndFractionalParts()
    {
        var integerPart = _subUnits / _divisor;
        var fractionalPart = _subUnits - (integerPart * _divisor);
        return (integerPart, fractionalPart);
    }

    public bool IsNaN()
    {
        return _isNaN;
    }

    public BigInteger GetSubUnits()
    {
        return _subUnits;
    }
}
