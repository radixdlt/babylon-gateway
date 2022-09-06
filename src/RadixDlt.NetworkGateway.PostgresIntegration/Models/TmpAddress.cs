using System.Text;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Models;

public class TmpAddress
{
    private readonly string _value;

    public TmpAddress(string value)
    {
        _value = value;
    }

    public static TmpAddress FromByteArray(byte[] input)
    {
        return new TmpAddress("bleh:" + input.Length);
    }

    public byte[] GetBytes()
    {
        return Encoding.UTF8.GetBytes(_value);
    }
}
