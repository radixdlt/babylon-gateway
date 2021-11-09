namespace Common.Addressing;

/// <summary>
/// An Exception thrown when parsing addresses.
/// </summary>
public class AddressException : Exception
{
    public AddressException(string message)
        : base(message)
    {
    }
}
