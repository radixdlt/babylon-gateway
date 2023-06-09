using Microsoft.Extensions.Logging;
using RadixDlt.NetworkGateway.Abstractions;
using RadixDlt.NetworkGateway.Abstractions.Addressing;
using RadixDlt.NetworkGateway.GatewayApi.Services;
using RadixDlt.RadixEngineToolkit.Model.Value.ScryptoSbor;
using System;
using System.Collections.Generic;
using System.Linq;
using Array = RadixDlt.RadixEngineToolkit.Model.Value.ScryptoSbor.Array;
using Enum = RadixDlt.RadixEngineToolkit.Model.Value.ScryptoSbor.Enum;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Services;

public interface IVirtualEntityMetadataProvider
{
    GatewayApiSdk.Model.EntityMetadataCollection GetVirtualEntityMetadata(EntityAddress virtualEntityAddress);
}

public class VirtualEntityMetadataProvider : IVirtualEntityMetadataProvider
{
    private readonly ILogger<VirtualEntityMetadataProvider> _logger;
    private readonly INetworkConfigurationProvider _networkConfigurationProvider;

    public VirtualEntityMetadataProvider(ILogger<VirtualEntityMetadataProvider> logger, INetworkConfigurationProvider networkConfigurationProvider)
    {
        _logger = logger;
        _networkConfigurationProvider = networkConfigurationProvider;
    }

    public GatewayApiSdk.Model.EntityMetadataCollection GetVirtualEntityMetadata(EntityAddress virtualEntityAddress)
    {
        var sbor = GetSbor(virtualEntityAddress);
        var encodedSbor = RadixEngineToolkit.RadixEngineToolkit.ScryptoSborEncode(sbor);
        var metadataItem = ScryptoSborUtils.MetadataValueToGatewayMetadataItemValue(_logger, encodedSbor, _networkConfigurationProvider.GetNetworkId());

        return new GatewayApiSdk.Model.EntityMetadataCollection(1, null, null, new List<GatewayApiSdk.Model.EntityMetadataItem> { new("owner_keys", metadataItem) });
    }

    private static IValue GetSbor(EntityAddress virtualEntityAddress)
    {
        var decodedAddress = RadixAddressCodec.Decode(virtualEntityAddress);

        if (decodedAddress.Data.Length != 30)
        {
            throw new NotSupportedException("Expected address to be 30 bytes length.");
        }

        var virtualSecp256k1 = new[] { 210, 209 };
        var virtualEd25519 = new[] { 81, 82 };

        var isVirtualSecp256k1 = virtualSecp256k1.Contains(decodedAddress.Data.First());
        var isVirtualEd25519 = virtualEd25519.Contains(decodedAddress.Data.First());

        if (!isVirtualSecp256k1 && !isVirtualEd25519)
        {
            throw new NotSupportedException("Failed to detect if it's virtualEd25519 or virtualSecp256k1");
        }

        var last29BytesOfAddress = decodedAddress.Data.Skip(1).Take(29).ToArray();
        var hexAddress = Convert.ToHexString(last29BytesOfAddress);

        IValue scryptoSbor = new Enum(
            1,
            new IValue[]
            {
                new Array(
                    ValueKind.Enum,
                    new IValue[]
                    {
                        new Enum(
                            15,
                            new IValue[]
                            {
                                new Enum(
                                    isVirtualSecp256k1 ? (byte)0 : (byte)1,
                                    new IValue[] { new Bytes(ValueKind.U8, hexAddress), }),
                            }),
                    }),
            }
        );

        return scryptoSbor;
    }
}
