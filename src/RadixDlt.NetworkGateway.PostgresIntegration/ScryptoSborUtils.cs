/* Copyright 2021 Radix Publishing Ltd incorporated in Jersey (Channel Islands).
 *
 * Licensed under the Radix License, Version 1.0 (the "License"); you may not use this
 * file except in compliance with the License. You may obtain a copy of the License at:
 *
 * radixfoundation.org/licenses/LICENSE-v1
 *
 * The Licensor hereby grants permission for the Canonical version of the Work to be
 * published, distributed and used under or by reference to the Licensor’s trademark
 * Radix ® and use of any unregistered trade names, logos or get-up.
 *
 * The Licensor provides the Work (and each Contributor provides its Contributions) on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied,
 * including, without limitation, any warranties or conditions of TITLE, NON-INFRINGEMENT,
 * MERCHANTABILITY, or FITNESS FOR A PARTICULAR PURPOSE.
 *
 * Whilst the Work is capable of being deployed, used and adopted (instantiated) to create
 * a distributed ledger it is your responsibility to test and validate the code, together
 * with all logic and performance of that code under all foreseeable scenarios.
 *
 * The Licensor does not make or purport to make and hereby excludes liability for all
 * and any representation, warranty or undertaking in any form whatsoever, whether express
 * or implied, to any entity or person, including any representation, warranty or
 * undertaking, as to the functionality security use, value or other characteristics of
 * any distributed ledger nor in respect the functioning or value of any tokens which may
 * be created stored or transferred using the Work. The Licensor does not warrant that the
 * Work or any use of the Work complies with any law or regulation in any territory where
 * it may be implemented or used or that it will be appropriate for any specific purpose.
 *
 * Neither the licensor nor any current or former employees, officers, directors, partners,
 * trustees, representatives, agents, advisors, contractors, or volunteers of the Licensor
 * shall be liable for any direct or indirect, special, incidental, consequential or other
 * losses of any kind, in tort, contract or otherwise (including but not limited to loss
 * of revenue, income or profits, or loss of use or data, or loss of reputation, or loss
 * of any economic or other opportunity of whatsoever nature or howsoever arising), arising
 * out of or in connection with (without limitation of any use, misuse, of any ledger system
 * or use made or its functionality or any performance or operation of any code or protocol
 * caused by bugs or programming or logic errors or otherwise);
 *
 * A. any offer, purchase, holding, use, sale, exchange or transmission of any
 * cryptographic keys, tokens or assets created, exchanged, stored or arising from any
 * interaction with the Work;
 *
 * B. any failure in a transmission or loss of any token or assets keys or other digital
 * artefacts due to errors in transmission;
 *
 * C. bugs, hacks, logic errors or faults in the Work or any communication;
 *
 * D. system software or apparatus including but not limited to losses caused by errors
 * in holding or transmitting tokens by any third-party;
 *
 * E. breaches or failure of security including hacker attacks, loss or disclosure of
 * password, loss of private key, unauthorised use or misuse of such passwords or keys;
 *
 * F. any losses including loss of anticipated savings or other benefits resulting from
 * use of the Work or any changes to the Work (however implemented).
 *
 * You are solely responsible for; testing, validating and evaluation of all operation
 * logic, functionality, security and appropriateness of using the Work for any commercial
 * or non-commercial purpose and for any reproduction or redistribution by You of the
 * Work. You assume all risks associated with Your use of the Work and the exercise of
 * permissions under this License.
 */

using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using RadixDlt.NetworkGateway.Abstractions.Extensions;
using RadixDlt.RadixEngineToolkit.Model.Exchange;
using RadixDlt.RadixEngineToolkit.Model.Value.ScryptoSbor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Array = RadixDlt.RadixEngineToolkit.Model.Value.ScryptoSbor.Array;
using Decimal = RadixDlt.RadixEngineToolkit.Model.Value.ScryptoSbor.Decimal;
using Enum = RadixDlt.RadixEngineToolkit.Model.Value.ScryptoSbor.Enum;
using GatewayModel = RadixDlt.NetworkGateway.GatewayApiSdk.Model;
using String = RadixDlt.RadixEngineToolkit.Model.Value.ScryptoSbor.String;
using ToolkitModel = RadixDlt.RadixEngineToolkit.Model;
using Tuple = RadixDlt.RadixEngineToolkit.Model.Value.ScryptoSbor.Tuple;

namespace RadixDlt.NetworkGateway.PostgresIntegration;

internal static class ScryptoSborUtils
{
    public static string ConvertFromScryptoSborString(string input, byte networkId)
    {
        var result = RadixEngineToolkit.RadixEngineToolkit.SborDecode(Convert.FromHexString(input), networkId);

        if (result is not SborDecodeResponse.ScryptoSbor scryptoSbor)
        {
            throw new UnreachableException("Expected ScryptoSbor response");
        }

        if (scryptoSbor.Value is not String value)
        {
            throw new UnreachableException("Expected ScryptoSbor.String");
        }

        return value;
    }

    public static GatewayModel.EntityMetadataItemValue MetadataValueToGatewayMetadataItemValue(ILogger logger, byte[] rawScryptoSbor, byte networkId)
    {
        var result = RadixEngineToolkit.RadixEngineToolkit.SborDecode(rawScryptoSbor, networkId);

        if (result is not SborDecodeResponse.ScryptoSbor scryptoSbor)
        {
            throw new UnreachableException("Expected ScryptoSbor response");
        }

        if (scryptoSbor.Value is not Enum metadataEntry)
        {
            throw new UnreachableException("Expected ScryptoSbor.Enum");
        }

        string? asString = null;
        List<string>? asStringCollection = null;

        switch (metadataEntry.Variant)
        {
            case 0 when metadataEntry.Fields is [Enum variantEnum]:
                asString = GetSimpleStringOfMetadataValue(logger, variantEnum);
                break;
            case 1 when metadataEntry.Fields is [Array innerArray]:
                if (innerArray.ElementKind == ValueKind.Enum)
                {
                    // For RCNet, Dashboard would rather have asString also populated for arrays
                    // For Mainnet, we may wish to give more structured metadata values from the Gateway API
                    asStringCollection = innerArray.Elements.OfType<Enum>().Select(variantEnum => GetSimpleStringOfMetadataValue(logger, variantEnum)).ToList();
                    asString = string.Join(", ", asStringCollection);
                }

                break;
        }

        if (asString == null)
        {
            logger.LogWarning("Unknown MetadataEntry variant: {}", metadataEntry.Variant);
            asString = "[UnrecognizedMetadataEntry]";
        }

        return new GatewayModel.EntityMetadataItemValue(
            rawHex: rawScryptoSbor.ToHex(),
            rawJson: new JRaw(RadixEngineToolkit.RadixEngineToolkit.ScryptoSborEncodeJson(metadataEntry)),
            asString: asString,
            asStringCollection: asStringCollection);
    }

    public static string GetSimpleStringOfMetadataValue(ILogger logger, Enum variantEnum)
    {
        switch (variantEnum.Variant)
        {
            // See https://github.com/radixdlt/radixdlt-scrypto/blob/release/rcnet-v1/transaction/examples/metadata/metadata.rtm
            case 0 when variantEnum.Fields is [String value]:
                return value.Value;
            case 1 when variantEnum.Fields is [Bool value]:
                return value.Value.ToString();
            case 2 when variantEnum.Fields is [U8 value]:
                return value.Value.ToString();
            case 3 when variantEnum.Fields is [U32 value]:
                return value.Value.ToString();
            case 4 when variantEnum.Fields is [U64 value]:
                return value.Value.ToString();
            case 5 when variantEnum.Fields is [I32 value]:
                return value.Value.ToString();
            case 6 when variantEnum.Fields is [I64 value]:
                return value.Value.ToString();
            case 7 when variantEnum.Fields is [Decimal value]:
                return value.Value;
            case 8 when variantEnum.Fields is [Address value]:
                return value.TmpAddress;
            case 9 when variantEnum.Fields is [Enum publicKeyEnum]:
                var keyName = publicKeyEnum.Variant switch
                {
                    0 => "EcdsaSecp256k1PublicKey",
                    1 => "EddsaEd25519PublicKey",
                    _ => $"PublicKeyType[{publicKeyEnum.Variant}]", // Fallback
                };

                if (publicKeyEnum.Fields is [Array keyBytes])
                {
                    try
                    {
                        var bytes = keyBytes.Elements.Cast<U8>().Select(byteValue => byteValue.Value).ToArray();
                        return $"{keyName}(\"{Convert.ToHexString(bytes).ToLowerInvariant()}\")";
                    }
                    catch (InvalidCastException)
                    {
                        // Fallthrough to default
                    }
                }

                break;
            case 10 when variantEnum.Fields is [Tuple nonFungibleGlobalId]:
                if (nonFungibleGlobalId.Elements is [Address nonFungibleResourceAddress, NonFungibleLocalId nonFungibleLocalId])
                {
                    return $"{nonFungibleResourceAddress.TmpAddress}:{FormatNonFungibleLocalId(nonFungibleLocalId.Value)}";
                }

                break;
            case 11 when variantEnum.Fields is [NonFungibleLocalId value]:
                return FormatNonFungibleLocalId(value.Value);
            case 12 when variantEnum.Fields is [Tuple instant]:
                if (instant.Elements is [I64 unixTimestampSeconds])
                {
                    return DateTimeOffset.FromUnixTimeSeconds(unixTimestampSeconds.Value).AsUtcIsoDateAtSecondsPrecisionString();
                }

                break;
            case 13 when variantEnum.Fields is [String url]:
                return url.Value;
        }

        logger.LogWarning("MetadataValue variant could not be mapped successfully: {}", variantEnum.Variant);
        return "[UnrecognizedMetadataValue]";
    }

    public static string FormatNonFungibleLocalId(ToolkitModel.INonFungibleLocalId nonFungibleLocalId)
    {
        switch (nonFungibleLocalId)
        {
            case ToolkitModel.INonFungibleLocalId.Bytes bytes:
                return $"[{Convert.ToHexString(bytes.Value).ToLowerInvariant()}]";
            case ToolkitModel.INonFungibleLocalId.Integer integer:
                return $"#{integer.Value}#";
            case ToolkitModel.INonFungibleLocalId.String s:
                return $"<{s.Value}>";
            case ToolkitModel.INonFungibleLocalId.UUID uuid:
                // Checked that this matches the representation in the Engine.
                // EG 5c220001220b01c0031c8cb574c04c44b2aa87263a00000000 should be {1c8cb574-c04c-44b2-aa87-263a00000000}
                // This should probably be lifted into the toolkit wrapper and a Guid be wrapped.
                return Guid.ParseExact(Convert.ToHexString(BigInteger.Parse(uuid.Value).ToByteArray(isUnsigned: true, isBigEndian: true)), "N").ToString("B");
            default:
                throw new ArgumentOutOfRangeException(nameof(nonFungibleLocalId));
        }
    }

    public static GatewayModel.ScryptoSborValue NonFungibleDataToGatewayScryptoSbor(byte[] rawScryptoSbor, byte networkId)
    {
        var result = RadixEngineToolkit.RadixEngineToolkit.SborDecode(rawScryptoSbor, networkId);

        if (result is not SborDecodeResponse.ScryptoSbor scryptoSbor)
        {
            throw new UnreachableException("Expected ScryptoSbor response");
        }

        return new GatewayModel.ScryptoSborValue(
            rawHex: rawScryptoSbor.ToHex(),
            rawJson: new JRaw(RadixEngineToolkit.RadixEngineToolkit.ScryptoSborEncodeJson(scryptoSbor.Value)));
    }
}
