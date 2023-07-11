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

using Newtonsoft.Json.Linq;
using RadixDlt.NetworkGateway.Abstractions.Extensions;
using RadixEngineToolkit;
using System;
using System.Linq;
using GatewayModel = RadixDlt.NetworkGateway.GatewayApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration;

internal static class ScryptoSborUtils
{
     public static string GetNonFungibleId(string input)
     {
         var decodedNfid = RadixEngineToolkitUniffiMethods.NonFungibleLocalIdSborDecode(Convert.FromHexString(input).ToList());
         var stringNfid = RadixEngineToolkitUniffiMethods.NonFungibleLocalIdAsStr(decodedNfid);
         return stringNfid;
     }

     public static GatewayModel.ScryptoSborValue NonFungibleDataToGatewayScryptoSbor(byte[] rawScryptoSbor, byte networkId)
     {
         var stringRepresentation = RadixEngineToolkitUniffiMethods.SborDecodeToStringRepresentation(rawScryptoSbor.ToList(), SerializationMode.PROGRAMMATIC, networkId, null);

         return new GatewayModel.ScryptoSborValue(
             rawHex: rawScryptoSbor.ToHex(),
             rawJson: JObject.Parse(stringRepresentation)
             );
     }

     public static GatewayModel.MetadataTypedValue DecodeToGatewayMetadataItemValue(byte[] rawScryptoSbor, byte networkId)
     {
         using var metadataValue = RadixEngineToolkitUniffiMethods.MetadataSborDecode(rawScryptoSbor.ToList(), networkId);
         return ConvertToolkitMetadataToGateway(metadataValue);
     }

     public static GatewayModel.MetadataTypedValue ConvertToolkitMetadataToGateway(MetadataValue metadataValue)
    {
        switch (metadataValue)
        {
            case MetadataValue.BoolArrayValue boolArrayValue:
                return new GatewayModel.MetadataScalarArrayValue(boolArrayValue.value.Select(x => x.ToString()).ToList(), GatewayModel.MetadataValueType.BoolArray);
            case MetadataValue.BoolValue boolValue:
                return new GatewayModel.MetadataScalarValue(boolValue.value.ToString(), GatewayModel.MetadataValueType.Bool);
            case MetadataValue.DecimalArrayValue decimalArrayValue:
                return new GatewayModel.MetadataScalarArrayValue(decimalArrayValue.value.Select(x => x.ToString()).ToList(), GatewayModel.MetadataValueType.DecimalArray);
            case MetadataValue.DecimalValue decimalValue:
                return new GatewayModel.MetadataScalarValue(decimalValue.value.ToString(), GatewayModel.MetadataValueType.Decimal);
            case MetadataValue.GlobalAddressArrayValue globalAddressArrayValue:
                return new GatewayModel.MetadataScalarArrayValue(globalAddressArrayValue.value.Select(x => x.AddressString()).ToList(), GatewayModel.MetadataValueType.GlobalAddressArray);
            case MetadataValue.GlobalAddressValue globalAddressValue:
                return new GatewayModel.MetadataScalarValue(globalAddressValue.value.AddressString(), GatewayModel.MetadataValueType.GlobalAddress);
            case MetadataValue.I32ArrayValue i32ArrayValue:
                return new GatewayModel.MetadataScalarArrayValue(i32ArrayValue.value.Select(x => x.ToString()).ToList(), GatewayModel.MetadataValueType.I32Array);
            case MetadataValue.I32Value i32Value:
                return new GatewayModel.MetadataScalarValue(i32Value.value.ToString(), GatewayModel.MetadataValueType.I32);
            case MetadataValue.I64ArrayValue i64ArrayValue:
                return new GatewayModel.MetadataScalarArrayValue(i64ArrayValue.value.Select(x => x.ToString()).ToList(), GatewayModel.MetadataValueType.I64Array);
            case MetadataValue.I64Value i64Value:
                return new GatewayModel.MetadataScalarValue(i64Value.value.ToString(), GatewayModel.MetadataValueType.I64);
            case MetadataValue.InstantArrayValue instantArrayValue:
                return new GatewayModel.MetadataScalarArrayValue(instantArrayValue.value.Select(x => DateTimeOffset.FromUnixTimeSeconds(x).AsUtcIsoDateAtSecondsPrecisionString()).ToList(), GatewayModel.MetadataValueType.InstantArray);
            case MetadataValue.InstantValue instantValue:
                return new GatewayModel.MetadataScalarValue(DateTimeOffset.FromUnixTimeSeconds(instantValue.value).AsUtcIsoDateAtSecondsPrecisionString(), GatewayModel.MetadataValueType.Instant);
            case MetadataValue.NonFungibleGlobalIdArrayValue nonFungibleGlobalIdArrayValue:
                return new GatewayModel.MetadataNonFungibleGlobalIdArrayValue(nonFungibleGlobalIdArrayValue.value.Select(x => new GatewayModel.MetadataNonFungibleGlobalIdValueAllOf(x.ResourceAddress().AddressString(), x.LocalId().ToString())).ToList());
            case MetadataValue.NonFungibleGlobalIdValue nonFungibleGlobalIdValue:
                return new GatewayModel.MetadataNonFungibleGlobalIdValue(nonFungibleGlobalIdValue.value.ResourceAddress().AddressString(), nonFungibleGlobalIdValue.value.LocalId().ToString());
            case MetadataValue.NonFungibleLocalIdArrayValue nonFungibleLocalIdArrayValue:
                return new GatewayModel.MetadataScalarArrayValue(nonFungibleLocalIdArrayValue.value.Select(x => RadixEngineToolkitUniffiMethods.NonFungibleLocalIdAsStr(x)).ToList(), GatewayModel.MetadataValueType.NonFungibleLocalIdArray);
            case MetadataValue.NonFungibleLocalIdValue nonFungibleLocalIdValue:
                var stringRepresentation = RadixEngineToolkitUniffiMethods.NonFungibleLocalIdAsStr(nonFungibleLocalIdValue.value);
                return new GatewayModel.MetadataScalarValue(stringRepresentation, GatewayModel.MetadataValueType.NonFungibleLocalId);
            case MetadataValue.OriginArrayValue originArrayValue:
                return new GatewayModel.MetadataScalarArrayValue(originArrayValue.value.Select(x => x.ToString()).ToList(), GatewayModel.MetadataValueType.OriginArray);
            case MetadataValue.OriginValue originValue:
                return new GatewayModel.MetadataScalarValue(originValue.value, GatewayModel.MetadataValueType.Origin);
            case MetadataValue.PublicKeyArrayValue publicKeyArrayValue:
                var publicKeyArrayCasted = publicKeyArrayValue.value.Select(x =>
                {
                    return x switch
                    {
                        PublicKey.Secp256k1 secp256k1 => secp256k1.value.ToArray().ToHex(),
                        PublicKey.Ed25519 ed25519 => ed25519.value.ToArray().ToHex(),
                        _ => throw new NotSupportedException($"Not expected public key type {x.GetType()}"),
                    };
                }).ToList();

                return new GatewayModel.MetadataScalarArrayValue(publicKeyArrayCasted, GatewayModel.MetadataValueType.PublicKeyArray);
            case MetadataValue.PublicKeyValue publicKeyValue:
                return publicKeyValue.value switch
                {
                    PublicKey.Secp256k1 secp256K1 => new GatewayModel.MetadataScalarValue(secp256K1.value.ToArray().ToHex(), GatewayModel.MetadataValueType.PublicKey),
                    PublicKey.Ed25519 ed25519 => new GatewayModel.MetadataScalarValue(ed25519.value.ToArray().ToHex(), GatewayModel.MetadataValueType.PublicKey),
                    _ => throw new NotSupportedException($"Not expected public key type {publicKeyValue.GetType()}"),
                };
            case MetadataValue.PublicKeyHashArrayValue publicKeyHashArray:
                var publicKeyHashArrayCasted = publicKeyHashArray.value.Select(x =>
                {
                    return x switch
                    {
                        PublicKeyHash.Secp256k1 secp256k1Hash => secp256k1Hash.value.ToArray().ToHex(),
                        PublicKeyHash.Ed25519 ed25519Hash => ed25519Hash.value.ToArray().ToHex(),
                        _ => throw new NotSupportedException($"Not expected public key type {x.GetType()}"),
                    };
                }).ToList();

                return new GatewayModel.MetadataScalarArrayValue(publicKeyHashArrayCasted, GatewayModel.MetadataValueType.PublicKeyHashArray);
            case MetadataValue.PublicKeyHashValue publicKeyHashValue:
                return publicKeyHashValue.value switch
                {
                    PublicKeyHash.Secp256k1 secp256k1Hash => new GatewayModel.MetadataScalarValue(secp256k1Hash.value.ToArray().ToHex(), GatewayModel.MetadataValueType.PublicKey),
                    PublicKeyHash.Ed25519 ed25519Hash => new GatewayModel.MetadataScalarValue(ed25519Hash.value.ToArray().ToHex(), GatewayModel.MetadataValueType.PublicKey),
                    _ => throw new NotSupportedException($"Not expected public key type {publicKeyHashValue.GetType()}"),
                };
            case MetadataValue.StringArrayValue stringArrayValue:
                return new GatewayModel.MetadataScalarArrayValue(stringArrayValue.value, GatewayModel.MetadataValueType.StringArray);
            case MetadataValue.StringValue stringValue:
                return new GatewayModel.MetadataScalarValue(stringValue.value, GatewayModel.MetadataValueType.String);
            case MetadataValue.U32ArrayValue u32ArrayValue:
                return new GatewayModel.MetadataScalarArrayValue(u32ArrayValue.value.Select(x => x.ToString()).ToList(), GatewayModel.MetadataValueType.U32Array);
            case MetadataValue.U32Value u32Value:
                return new GatewayModel.MetadataScalarValue(u32Value.value.ToString(), GatewayModel.MetadataValueType.U32);
            case MetadataValue.U64ArrayValue u64ArrayValue:
                return new GatewayModel.MetadataScalarArrayValue(u64ArrayValue.value.Select(x => x.ToString()).ToList(), GatewayModel.MetadataValueType.U64Array);
            case MetadataValue.U64Value u64Value:
                return new GatewayModel.MetadataScalarValue(u64Value.value.ToString(), GatewayModel.MetadataValueType.U64);
            case MetadataValue.U8ArrayValue u8ArrayValue:
                return new GatewayModel.MetadataScalarArrayValue(u8ArrayValue.value.Select(x => x.ToString()).ToList(), GatewayModel.MetadataValueType.U8Array);
            case MetadataValue.U8Value u8Value:
                return new GatewayModel.MetadataScalarValue(u8Value.value.ToString(), GatewayModel.MetadataValueType.U8);
            case MetadataValue.UrlArrayValue urlArrayValue:
                return new GatewayModel.MetadataScalarArrayValue(urlArrayValue.value, GatewayModel.MetadataValueType.UrlArray);
            case MetadataValue.UrlValue urlValue:
                return new GatewayModel.MetadataScalarValue(urlValue.value, GatewayModel.MetadataValueType.Url);
            default:
                throw new NotSupportedException($"Unexpected metadataValue type {metadataValue.GetType()}");
        }
    }
}
