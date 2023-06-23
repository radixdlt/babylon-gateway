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
using System.Diagnostics;
using System.Linq;
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
    public static GatewayModel.EntityMetadataItemValue MetadataValueToGatewayMetadataItemValue(byte[] rawScryptoSbor, byte networkId, ILogger logger)
    {
        var result = RadixEngineToolkit.RadixEngineToolkit.SborDecode(rawScryptoSbor, networkId);

        if (result is not SborDecodeResponse.ScryptoSbor scryptoSbor)
        {
            throw new UnreachableException($"Expected ScryptoSbor response, rawBytes={rawScryptoSbor.ToHex()}");
        }

        if (scryptoSbor.Value is not Enum @enum)
        {
            throw new UnreachableException($"Expected ScryptoSbor.Enum, rawBytes={rawScryptoSbor.ToHex()}");
        }

        GatewayModel.MetadataTypedValue? typedValue = null;

        switch (@enum.VariantId)
        {
            case 0 when @enum.Fields is [String value]:
                typedValue = new GatewayModel.MetadataScalarValue(value, GatewayModel.MetadataValueType.String);
                break;
            case 1 when @enum.Fields is [Bool value]:
                typedValue = new GatewayModel.MetadataScalarValue(value.Value.ToString(), GatewayModel.MetadataValueType.Bool);
                break;
            case 2 when @enum.Fields is [U8 value]:
                typedValue = new GatewayModel.MetadataScalarValue(value.Value.ToString(), GatewayModel.MetadataValueType.U8);
                break;
            case 3 when @enum.Fields is [U32 value]:
                typedValue = new GatewayModel.MetadataScalarValue(value.Value.ToString(), GatewayModel.MetadataValueType.U32);
                break;
            case 4 when @enum.Fields is [U64 value]:
                typedValue = new GatewayModel.MetadataScalarValue(value.Value.ToString(), GatewayModel.MetadataValueType.U64);
                break;
            case 5 when @enum.Fields is [I32 value]:
                typedValue = new GatewayModel.MetadataScalarValue(value.Value.ToString(), GatewayModel.MetadataValueType.I32);
                break;
            case 6 when @enum.Fields is [I64 value]:
                typedValue = new GatewayModel.MetadataScalarValue(value.Value.ToString(), GatewayModel.MetadataValueType.I64);
                break;
            case 7 when @enum.Fields is [Decimal value]:
                typedValue = new GatewayModel.MetadataScalarValue(value.Value, GatewayModel.MetadataValueType.Decimal);
                break;
            case 8 when @enum.Fields is [Reference reference]:
                typedValue = new GatewayModel.MetadataScalarValue(reference.Value, GatewayModel.MetadataValueType.GlobalAddress);
                break;
            case 9 when @enum.Fields is [Enum publicKeyEnum]:
                if (publicKeyEnum.Fields is [Bytes keyBytes])
                {
                    GatewayModel.PublicKey? pk = publicKeyEnum.VariantId switch
                    {
                        0 => new GatewayModel.PublicKeyEcdsaSecp256k1(((byte[])keyBytes.Hex).ToHex()),
                        1 => new GatewayModel.PublicKeyEddsaEd25519(((byte[])keyBytes.Hex).ToHex()),
                        _ => null,
                    };

                    if (pk != null)
                    {
                        typedValue = new GatewayModel.MetadataPublicKeyValue(pk);
                    }
                }

                break;
            case 10 when @enum.Fields is [Tuple nonFungibleGlobalId]:
                if (nonFungibleGlobalId.Fields is [Reference nonFungibleResourceAddress, NonFungibleLocalId nonFungibleLocalId])
                {
                    typedValue = new GatewayModel.MetadataNonFungibleGlobalIdValue(nonFungibleResourceAddress.Value, nonFungibleLocalId.Value);
                }

                break;
            case 11 when @enum.Fields is [NonFungibleLocalId value]:
                typedValue = new GatewayModel.MetadataScalarValue(value.Value, GatewayModel.MetadataValueType.NonFungibleLocalId);
                break;
            case 12 when @enum.Fields is [I64 instant]:
                typedValue = new GatewayModel.MetadataScalarValue(DateTimeOffset.FromUnixTimeSeconds(instant.Value).AsUtcIsoDateAtSecondsPrecisionString(), GatewayModel.MetadataValueType.Instant);
                break;
            case 13 when @enum.Fields is [String url]:
                typedValue = new GatewayModel.MetadataScalarValue(url.Value, GatewayModel.MetadataValueType.Url);
                break;
            case 14 when @enum.Fields is [String origin]:
                typedValue = new GatewayModel.MetadataScalarValue(origin.Value, GatewayModel.MetadataValueType.Origin);
                break;
            case 15 when @enum.Fields is [Enum publicKeyHashEnum]:
                if (publicKeyHashEnum.Fields is [Bytes hashKeyBytes])
                {
                    typedValue = new GatewayModel.MetadataScalarValue(((byte[])hashKeyBytes.Hex).ToHex(), GatewayModel.MetadataValueType.PublicKeyHash);
                }

                break;
        }

        // arrays use same variants as their scalar counterparts with 0x80 being added to them
        if ((@enum.VariantId & 0x80) != 0)
        {
            var scalarVariant = (byte)(@enum.VariantId & (~0x80));

            if (@enum.Fields is [Array array])
            {
                switch (scalarVariant)
                {
                    case 0 when array.Elements.All(e => e is String):
                        typedValue = new GatewayModel.MetadataScalarArrayValue(array.Elements.Cast<String>().Select(e => e.Value).ToList(), GatewayModel.MetadataValueType.StringArray);
                        break;
                    case 1 when array.Elements.All(e => e is Bool):
                        typedValue = new GatewayModel.MetadataScalarArrayValue(array.Elements.Cast<Bool>().Select(e => e.Value.ToString()).ToList(), GatewayModel.MetadataValueType.BoolArray);
                        break;
                    case 2 when array.Elements.All(e => e is U8):
                        typedValue = new GatewayModel.MetadataScalarValue(array.Elements.Cast<U8>().Select(e => e.Value).ToArray().ToHex(), GatewayModel.MetadataValueType.U8Array);
                        break;
                    case 3 when array.Elements.All(e => e is U32):
                        typedValue = new GatewayModel.MetadataScalarArrayValue(array.Elements.Cast<U32>().Select(e => e.Value.ToString()).ToList(), GatewayModel.MetadataValueType.U32Array);
                        break;
                    case 4 when array.Elements.All(e => e is U64):
                        typedValue = new GatewayModel.MetadataScalarArrayValue(array.Elements.Cast<U64>().Select(e => e.Value.ToString()).ToList(), GatewayModel.MetadataValueType.U64Array);
                        break;
                    case 5 when array.Elements.All(e => e is I32):
                        typedValue = new GatewayModel.MetadataScalarArrayValue(array.Elements.Cast<I32>().Select(e => e.Value.ToString()).ToList(), GatewayModel.MetadataValueType.I32Array);
                        break;
                    case 6 when array.Elements.All(e => e is I64):
                        typedValue = new GatewayModel.MetadataScalarArrayValue(array.Elements.Cast<I64>().Select(e => e.Value.ToString()).ToList(), GatewayModel.MetadataValueType.I64Array);
                        break;
                    case 7 when array.Elements.All(e => e is Decimal):
                        typedValue = new GatewayModel.MetadataScalarArrayValue(array.Elements.Cast<Decimal>().Select(e => e.Value.ToString()).ToList(), GatewayModel.MetadataValueType.DecimalArray);
                        break;
                    case 8 when array.Elements.All(e => e is Reference):
                        typedValue = new GatewayModel.MetadataScalarArrayValue(array.Elements.Cast<Reference>().Select(e => e.Value.ToString()).ToList(), GatewayModel.MetadataValueType.GlobalAddressArray);
                        break;
                    case 9 when array.Elements.All(e => e is Enum):
                        var pks = array.Elements
                            .Cast<Enum>()
                            .Select(e =>
                            {
                                if (e.Fields is [Bytes keyBytes])
                                {
                                    GatewayModel.PublicKey? pk = e.VariantId switch
                                    {
                                        0 => new GatewayModel.PublicKeyEcdsaSecp256k1(((byte[])keyBytes.Hex).ToHex()),
                                        1 => new GatewayModel.PublicKeyEddsaEd25519(((byte[])keyBytes.Hex).ToHex()),
                                        _ => null,
                                    };

                                    return pk;
                                }

                                return null;
                            })
                            .ToList();

                        if (pks.Any(pk => pk == null))
                        {
                            break;
                        }

                        typedValue = new GatewayModel.MetadataPublicKeyArrayValue(pks);
                        break;
                    case 10 when array.Elements.All(e => e is Tuple):
                        var nfg = array.Elements
                            .Cast<Tuple>()
                            .Select(e =>
                            {
                                if (e.Fields is [Reference nonFungibleResourceAddress, NonFungibleLocalId nonFungibleLocalId])
                                {
                                    return new GatewayModel.MetadataNonFungibleGlobalIdValueAllOf(nonFungibleResourceAddress.Value, nonFungibleLocalId.Value);
                                }

                                return null;
                            })
                            .ToList();

                        if (nfg.Any(e => e == null))
                        {
                            break;
                        }

                        typedValue = new GatewayModel.MetadataNonFungibleGlobalIdArrayValue(nfg);
                        break;
                    case 11 when array.Elements.All(e => e is NonFungibleLocalId):
                        typedValue = new GatewayModel.MetadataScalarArrayValue(array.Elements.Cast<NonFungibleLocalId>().Select(e => e.Value.ToString()).ToList(), GatewayModel.MetadataValueType.NonFungibleLocalIdArray);
                        break;
                    case 12 when array.Elements.All(e => e is I64):
                        typedValue = new GatewayModel.MetadataScalarArrayValue(array.Elements.Cast<I64>().Select(e => DateTimeOffset.FromUnixTimeSeconds(e.Value).AsUtcIsoDateAtSecondsPrecisionString()).ToList(), GatewayModel.MetadataValueType.InstantArray);
                        break;
                    case 13 when array.Elements.All(e => e is String):
                        typedValue = new GatewayModel.MetadataScalarArrayValue(array.Elements.Cast<String>().Select(e => e.Value.ToString()).ToList(), GatewayModel.MetadataValueType.UrlArray);
                        break;
                    case 14 when array.Elements.All(e => e is String):
                        typedValue = new GatewayModel.MetadataScalarArrayValue(array.Elements.Cast<String>().Select(e => e.Value.ToString()).ToList(), GatewayModel.MetadataValueType.OriginArray);
                        break;
                    case 15 when array.Elements.All(e => e is Enum):
                        var h = array.Elements
                            .Cast<Enum>()
                            .Select(e =>
                            {
                                if (e.Fields is [Bytes hashKeyBytes])
                                {
                                    return ((byte[])hashKeyBytes.Hex).ToHex();
                                }

                                return null;
                            })
                            .ToList();

                        if (h.Any(e => e == null))
                        {
                            break;
                        }

                        typedValue = new GatewayModel.MetadataScalarArrayValue(h, GatewayModel.MetadataValueType.PublicKeyHashArray);
                        break;
                }
            }
            else if (scalarVariant == 2 && @enum.Fields is [Bytes bytes])
            {
                // alternate encoding for array of u8s
                typedValue = new GatewayModel.MetadataScalarValue(((byte[])bytes.Hex).ToHex(), GatewayModel.MetadataValueType.U8Array);
            }
        }

        if (typedValue == null)
        {
            logger.LogWarning("Unknown MetadataEntry variantId={VariantId}, rawBytes={RawBytes}", @enum.VariantId, rawScryptoSbor.ToHex());
        }

        return new GatewayModel.EntityMetadataItemValue(
            rawHex: rawScryptoSbor.ToHex(),
            rawJson: new JRaw(RadixEngineToolkit.RadixEngineToolkit.ScryptoSborEncodeJson(@enum)),
            typed: typedValue);
    }
}
