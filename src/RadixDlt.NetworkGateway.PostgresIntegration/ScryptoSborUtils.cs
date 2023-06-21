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
using System.Diagnostics.CodeAnalysis;
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
    public static string GetNonFungibleId(string input, byte networkId)
    {
        var result = RadixEngineToolkit.RadixEngineToolkit.SborDecode(Convert.FromHexString(input), networkId);

        if (result is not SborDecodeResponse.ScryptoSbor scryptoSbor)
        {
            throw new UnreachableException("Expected ScryptoSbor response");
        }

        if (scryptoSbor.Value is not NonFungibleLocalId nonFungibleLocalId)
        {
            throw new UnreachableException("Expected ScryptoSbor.NonFungibleLocalId");
        }

        return nonFungibleLocalId.Value;
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

    // See: https://github.com/radixdlt/radixdlt-scrypto/blob/release/birch/radix-engine-interface/src/api/node_modules/metadata/invocations.rs
    public static GatewayModel.EntityMetadataItemValue MetadataValueToGatewayMetadataItemValue(ILogger logger, byte[] rawScryptoSbor, byte networkId)
    {
        var result = RadixEngineToolkit.RadixEngineToolkit.SborDecode(rawScryptoSbor, networkId);

        if (result is not SborDecodeResponse.ScryptoSbor scryptoSbor)
        {
            throw new UnreachableException("Expected ScryptoSbor response");
        }

        if (scryptoSbor.Value is not Enum outerEnum)
        {
            throw new UnreachableException("Expected ScryptoSbor.Enum");
        }

        string? asString = null;
        List<string>? asStringCollection = null;

        // if we're dealing with array value...
        if ((outerEnum.VariantId & 0x80) != 0 && outerEnum.Fields is [Array value])
        {
            // ...we can reuse scalar-specific logic by wrapping actual values in their respective enum counterparts

            var enumVariant = (byte)(outerEnum.VariantId & (~0x80));

            foreach (var arrayElement in value.Elements)
            {
                if (TryGetSimpleStringOfMetadataValue(new Enum(enumVariant, new[] { arrayElement }), out var stringValue))
                {
                    asStringCollection ??= new List<string>();
                    asStringCollection.Add(stringValue);
                }
                else
                {
                    logger.LogWarning("Unknown MetadataEntry variantId={VariantId}, rawBytes={RawBytes}", outerEnum.VariantId, rawScryptoSbor.ToHex());
                }
            }
        }
        else if (TryGetSimpleStringOfMetadataValue(outerEnum, out var stringValue))
        {
            asString = stringValue;
        }

        if (asString == null && asStringCollection == null)
        {
            logger.LogWarning("Unknown MetadataEntry variantId={VariantId}, rawBytes={RawBytes}", outerEnum.VariantId, rawScryptoSbor.ToHex());
        }

        return new GatewayModel.EntityMetadataItemValue(
            rawHex: rawScryptoSbor.ToHex(),
            rawJson: new JRaw(RadixEngineToolkit.RadixEngineToolkit.ScryptoSborEncodeJson(outerEnum)),
            asString: asString,
            asStringCollection: asStringCollection);
    }

    private static bool TryGetSimpleStringOfMetadataValue(Enum @enum, [NotNullWhen(true)] out string? result)
    {
        result = null;

        switch (@enum.VariantId)
        {
            case 0 when @enum.Fields is [String value]:
                result = value.Value;
                break;
            case 1 when @enum.Fields is [Bool value]:
                result = value.Value.ToString();
                break;
            case 2 when @enum.Fields is [U8 value]:
                result = value.Value.ToString();
                break;
            case 3 when @enum.Fields is [U32 value]:
                result = value.Value.ToString();
                break;
            case 4 when @enum.Fields is [U64 value]:
                result = value.Value.ToString();
                break;
            case 5 when @enum.Fields is [I32 value]:
                result = value.Value.ToString();
                break;
            case 6 when @enum.Fields is [I64 value]:
                result = value.Value.ToString();
                break;
            case 7 when @enum.Fields is [Decimal value]:
                result = value.Value;
                break;
            case 8 when @enum.Fields is [Reference reference]:
                result = reference.Value;
                break;
            case 9 when @enum.Fields is [Enum publicKeyEnum]:
                var keyName = publicKeyEnum.VariantId switch
                {
                    0 => "EcdsaSecp256k1PublicKey",
                    1 => "EddsaEd25519PublicKey",
                    _ => $"PublicKeyType[{publicKeyEnum.VariantId}]", // Fallback
                };

                if (publicKeyEnum.Fields is [Bytes keyBytes])
                {
                    result = $"{keyName}(\"{Convert.ToHexString(keyBytes.Hex).ToLowerInvariant()}\")";
                }

                break;
            case 10 when @enum.Fields is [Tuple nonFungibleGlobalId]:
                if (nonFungibleGlobalId.Fields is [Reference nonFungibleResourceAddress, NonFungibleLocalId nonFungibleLocalId])
                {
                    result = $"{nonFungibleResourceAddress.Value}:{nonFungibleLocalId.Value}";
                }

                break;
            case 11 when @enum.Fields is [NonFungibleLocalId value]:
                result = value.Value;
                break;
            case 12 when @enum.Fields is [I64 instant]:
                result = DateTimeOffset.FromUnixTimeSeconds(instant.Value).AsUtcIsoDateAtSecondsPrecisionString();
                break;
            case 13 when @enum.Fields is [String url]:
                result = url.Value;
                break;
            case 14 when @enum.Fields is [String origin]:
                result = origin.Value;
                break;
            case 15 when @enum.Fields is [Enum publicKeyHashEnum]:
                var hashKeyName = publicKeyHashEnum.VariantId switch
                {
                    0 => "EcdsaSecp256k1PublicKeyHash",
                    1 => "EddsaEd25519PublicKeyHash",
                    _ => $"PublicKeyHashType[{publicKeyHashEnum.VariantId}]", // Fallback
                };

                if (publicKeyHashEnum.Fields is [Bytes hashKeyBytes])
                {
                    result = $"{hashKeyName}(\"{Convert.ToHexString(hashKeyBytes.Hex).ToLowerInvariant()}\")";
                }

                break;
        }

        return result != null;
    }
}
