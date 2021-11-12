using System.Security.Cryptography;

namespace Common.StaticHelpers;

public static class AccumulatorVerifier
{
    public static bool IsValidAccumulator(byte[] parentAccumulator, byte[] childHash, byte[] newAccumulator)
    {
        if (parentAccumulator.Length != 32 || childHash.Length != 32 || newAccumulator.Length != 32)
        {
            return false;
        }

        Span<byte> aggregate = stackalloc byte[64];
        parentAccumulator.CopyTo(aggregate);
        childHash.CopyTo(aggregate[32..]);

        // Create result - Sha256Twice
        Span<byte> hashResult1 = stackalloc byte[32];
        SHA256.HashData(aggregate, hashResult1);
        Span<byte> hashResult2 = stackalloc byte[32];
        SHA256.HashData(hashResult1, hashResult2);

        // Compare
        return hashResult2.SequenceEqual(newAccumulator);
    }

    // NB - There is some repetition with the above for performance gains regarding stackalloc.
    // By using dynamic sizes we can ensure this always returns a value
    public static byte[] CreateNewAccumulator(byte[] parentAccumulator, byte[] childHash)
    {
        // Create Aggregate
        var totalLength = parentAccumulator.Length + childHash.Length;
        Span<byte> aggregate = totalLength > 256 ? new byte[totalLength] : stackalloc byte[totalLength];
        parentAccumulator.CopyTo(aggregate);
        childHash.CopyTo(aggregate[parentAccumulator.Length..]);

        // Create result - Sha256Twice
        Span<byte> hashResult1 = stackalloc byte[32];
        SHA256.HashData(aggregate, hashResult1);
        return SHA256.HashData(hashResult1);
    }
}
