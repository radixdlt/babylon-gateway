using System.Security.Cryptography;

namespace Common.StaticHelpers;

public static class HashingHelper
{
    public static void Sha256Twice(ReadOnlySpan<byte> source, Span<byte> destination)
    {
        Span<byte> hashResult1 = stackalloc byte[32];
        SHA256.HashData(source, hashResult1);
        SHA256.HashData(hashResult1, destination);
    }

    public static byte[] Sha256Twice(ReadOnlySpan<byte> source)
    {
        Span<byte> hashResult1 = stackalloc byte[32];
        SHA256.HashData(source, hashResult1);
        return SHA256.HashData(hashResult1);
    }
}
