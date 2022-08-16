using RadixDlt.NetworkGateway.Common.Numerics;
using System;
using System.Numerics;

namespace RadixDlt.NetworkGateway.Common.Extensions;

internal static class TokenAmountExtensions
{
    private const int MaxPostgresPrecision = 1000;

    private static readonly int _safeByteLengthLimitBeforePostgresError;

    static TokenAmountExtensions()
    {
        // We under-estimate this so that we're definitely safe
        _safeByteLengthLimitBeforePostgresError = (int)Math.Floor(MaxPostgresPrecision * Math.Log(10, 256));
    }

    /// <summary>
    /// Calculates the corresponding string for postgres handling, assuming decimal(1000, 18).
    /// If the number is too large, we return NaN so that we can still ingest the transaction.
    /// See <a href="https://www.postgresql.org/docs/14/datatype-numeric.html">the Postgres Numeric docs</a>.
    ///
    /// Unfortunately, NPGSQL is yet to support NaN in their BigInteger numeric fields.
    /// </summary>
    public static string ToPostgresDecimal(this TokenAmount tokenAmount)
    {
        return tokenAmount.IsNaN() || tokenAmount.IsUnsafeSizeForPostgres() ? TokenAmount.StringForNaN : tokenAmount.ToString();
    }

    public static BigInteger GetSubUnitsSafeForPostgres(this TokenAmount tokenAmount)
    {
        if (tokenAmount.IsNaN())
        {
            throw new ArithmeticException("TokenAmount is NaN, cannot get SubUnits.");
        }

        if (tokenAmount.IsUnsafeSizeForPostgres())
        {
            throw new ArithmeticException("TokenAmount is too large to persist to Postgres.");
        }

        return tokenAmount.GetSubUnits();
    }

    /// <summary>
    /// Calculates whether the string is certainly safe for postgres handling, assuming decimal(1000, 18).
    /// </summary>
    private static bool IsUnsafeSizeForPostgres(this TokenAmount tokenAmount)
    {
        return tokenAmount.GetSubUnits().GetByteCount(isUnsigned: false) >= _safeByteLengthLimitBeforePostgresError;
    }
}
