using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace RadixDlt.NetworkGateway.PostgresIntegration.ValueConverters;

internal class TmpEntityTypeValueConverter : EnumTypeValueConverterBase<TmpEntityType>
{
    public static readonly ImmutableDictionary<TmpEntityType, string> Conversion =
        new Dictionary<TmpEntityType, string>
        {
            { TmpEntityType.FungibleResource, "FUNGIBLE_RESOURCE" },
            { TmpEntityType.NonFungibleResource, "NON_FUNGIBLE_RESOURCE" },
            { TmpEntityType.Account, "ACCOUNT" },
            { TmpEntityType.Validator, "VALIDATOR" },
            { TmpEntityType.Vault, "VAULT" },
            { TmpEntityType.KeyValueStore, "KEY_VALUE_STORE" },
        }.ToImmutableDictionary();

    public TmpEntityTypeValueConverter()
        : base(Conversion, Invert(Conversion))
    {
    }
}
