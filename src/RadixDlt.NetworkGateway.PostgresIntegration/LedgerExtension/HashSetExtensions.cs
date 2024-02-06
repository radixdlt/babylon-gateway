using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension;

internal static class HashSetExtensions
{
    public static bool Unzip<TIn, TOut1, TOut2>(
        this HashSet<TIn> input,
        Func<TIn, TOut1> out1Selector,
        Func<TIn, TOut2> out2Selector,
        [NotNullWhen(true)] out List<TOut1>? out1,
        [NotNullWhen(true)] out List<TOut2>? out2)
    {
        out1 = default;
        out2 = default;

        if (!input.Any())
        {
            return false;
        }

        out1 = new List<TOut1>(input.Count);
        out2 = new List<TOut2>(input.Count);

        foreach (var e in input)
        {
            out1.Add(out1Selector(e));
            out2.Add(out2Selector(e));
        }

        return true;
    }
}
