using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RadixDlt.NetworkGateway.IntegrationTests.Data;
using System;
using System.Numerics;
using System.Reflection;

namespace RadixDlt.NetworkGateway.IntegrationTests.Utilities;

public static class TokenAttosConverter
{
    public static BigInteger CostUnitConsumed2Attos(long costUnitConsumed, string? costUnitPriceAttos = default(string))
    {
        if (string.IsNullOrWhiteSpace(costUnitPriceAttos))
        {
            costUnitPriceAttos = GenesisData.GenesisFeeSummary.CostUnitPriceAttos;
        }

        BigInteger.TryParse(costUnitPriceAttos, out var biCostUnitPriceAttos);

        return costUnitConsumed * biCostUnitPriceAttos;
    }

    public static BigInteger CostUnitConsumed2Token(long costUnitConsumed, string? costUnitPriceAttos = default(string), int divisibility = 18)
    {
        var costUnitConsumed2Attos = CostUnitConsumed2Attos(costUnitConsumed, costUnitPriceAttos);

        return costUnitConsumed2Attos / BigInteger.Pow(10, divisibility);
    }

    public static BigInteger Tokens2Attos(double tokens, int divisibility = 18)
    {
        return BigInteger.Parse(tokens.ToString()) * BigInteger.Pow(10, divisibility);
    }

    public static BigInteger Tokens2Attos(string strAttos, int divisibility = 18)
    {
        return Tokens2Attos(double.Parse(strAttos), divisibility);
    }

    public static double Attos2Tokens(BigInteger attos, int divisibility = 18)
    {
        return double.Parse(attos.ToString()) / Math.Pow(10, divisibility);
    }

    public static BigInteger ParseAttosFromString(string attos)
    {
        return BigInteger.Parse(attos);
    }

    public static double ParseTokensFromString(string tokens)
    {
        return double.Parse(tokens);
    }
}
