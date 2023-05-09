using System;
using System.Collections.Generic;

namespace RadixDlt.NetworkGateway.Abstractions;

public class EventTypeIdentifiers
{
    public EventTypeIdentifiers(
        VaultEventTypeIdentifiers vault,
        FungibleResourceEventTypeIdentifiers fungibleResource,
        NonFungibleResourceEventTypeIdentifiers nonFungibleResource)
    {
        Vault = vault;
        FungibleResource = fungibleResource;
        NonFungibleResource = nonFungibleResource;
    }

    public VaultEventTypeIdentifiers Vault { get; }

    public FungibleResourceEventTypeIdentifiers FungibleResource { get; }

    public NonFungibleResourceEventTypeIdentifiers NonFungibleResource { get; }

    public sealed record VaultEventTypeIdentifiers(int Withdrawal, int Deposit);

    public sealed record FungibleResourceEventTypeIdentifiers(int Minted, int Burned);

    public sealed record NonFungibleResourceEventTypeIdentifiers(int Minted, int Burned);

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        return obj.GetType() == GetType() && Equals((EventTypeIdentifiers)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Vault, FungibleResource, NonFungibleResource);
    }

    private bool Equals(EventTypeIdentifiers other)
    {
        return Vault.Equals(other.Vault) &&
               FungibleResource.Equals(other.FungibleResource) &&
               NonFungibleResource.Equals(other.NonFungibleResource);
    }
}
