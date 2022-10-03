namespace RadixDlt.NetworkGateway.Commons.Extensions;

public static class RadixAddressExtensions
{
    public static byte[]? AsByteArray(this RadixAddress? ra)
    {
        if (ra == null)
        {
            return null;
        }

        return (byte[])ra;
    }
}
