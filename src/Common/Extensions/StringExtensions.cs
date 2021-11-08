namespace Common.Extensions;

public static class StringExtensions
{
    public static string Truncate(this string str, int maxLength)
    {
        if (string.IsNullOrEmpty(str))
        {
            return str;
        }

        return str.Length <= maxLength ? str : str[..maxLength];
    }

    public static byte[] ConvertFromHex(this string str)
    {
        return Convert.FromHexString(str);
    }
}
