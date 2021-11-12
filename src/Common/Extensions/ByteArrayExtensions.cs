namespace Common.Extensions;

public static class ByteArrayExtensions
{
    public static bool BytesAreEqual(this byte[] array1, ReadOnlySpan<byte> array2)
    {
        return ((ReadOnlySpan<byte>)array1).SequenceEqual(array2);
    }

    public static string ToHex(this byte[] array)
    {
        return Convert.ToHexString(array);
    }
}
