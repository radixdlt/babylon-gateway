namespace RadixDlt.NetworkGateway.Abstractions;

public sealed record EventTypeIdentifiers(
    Vault Vault,
    FungibleResource FungibleResource,
    NonFungibleResource NonFungibleResource
);

public sealed record Vault(int Withdrawal, int Deposit);
public sealed record FungibleResource(int Minted, int Burned);
public sealed record NonFungibleResource(int Minted, int Burned);
