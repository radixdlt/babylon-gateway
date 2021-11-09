namespace Common.Addressing;

public enum RadixAddressType
{
    Account,
    Resource,
    Validator,
    Node,
}

public record RadixAddress(RadixAddressType Type, string Hrp, byte[] AddressData);
