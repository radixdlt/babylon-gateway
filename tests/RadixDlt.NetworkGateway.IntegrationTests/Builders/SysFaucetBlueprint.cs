namespace RadixDlt.NetworkGateway.IntegrationTests.Builders;

public interface IBlueprint
{
    public string Name { get; }

    void Instantiate(object[] args);
}

// inspired by: https://github.com/radixdlt/radixdlt-scrypto/blob/f4c41985ad9d1570fff7293fa90b77a896b3be2b/assets/sys-faucet/src/lib.rs
public class SysFaucetBlueprint : IBlueprint
{
    public string Name => "SysFaucet";

    public void Instantiate(object[] args)
    {
        // mock blueprint instantiation
    }

    // Gives away XRD tokens.
    public void Free_xrd()
    {
        // mock 'free_xrd'
    }

    // Locks fees
    public void Lock_fee(decimal amount)
    {
        // mock 'lock_fee
    }
}
