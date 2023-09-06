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

using RadixDlt.NetworkGateway.Abstractions.Extensions;
using RadixDlt.NetworkGateway.Abstractions.Model;
using System;
using System.Linq;
using GatewayModel = RadixDlt.NetworkGateway.GatewayApiSdk.Model;
using ToolkitModel = RadixEngineToolkit;

namespace RadixDlt.NetworkGateway.PostgresIntegration;

internal static class ScryptoSborUtils
{
    public static string GetNonFungibleId(string input)
    {
        var decodedNfid = ToolkitModel.RadixEngineToolkitUniffiMethods.NonFungibleLocalIdSborDecode(Convert.FromHexString(input));
        var stringNfid = ToolkitModel.RadixEngineToolkitUniffiMethods.NonFungibleLocalIdAsStr(decodedNfid);
        return stringNfid;
    }

    public static string DataToProgrammaticJson(byte[] data, byte[] schemaBytes, SborTypeKind keyTypeKind, long schemaIndex, byte networkId)
    {
        ToolkitModel.LocalTypeId typeIndex = keyTypeKind switch
        {
            SborTypeKind.SchemaLocal => new ToolkitModel.LocalTypeId.SchemaLocalIndex((ulong)schemaIndex),
            SborTypeKind.WellKnown => new ToolkitModel.LocalTypeId.WellKnown((byte)schemaIndex),
            _ => throw new ArgumentOutOfRangeException(nameof(keyTypeKind), keyTypeKind, null),
        };

        var schema = new ToolkitModel.Schema(typeIndex, schemaBytes);

        var stringRepresentation = ToolkitModel.RadixEngineToolkitUniffiMethods.SborDecodeToStringRepresentation(
            data,
            ToolkitModel.SerializationMode.PROGRAMMATIC,
            networkId,
            schema);

        return stringRepresentation;
    }

    public static GatewayModel.MetadataTypedValue DecodeToGatewayMetadataItemValue(byte[] rawScryptoSbor, byte networkId)
    {
        using var metadataValue = ToolkitModel.RadixEngineToolkitUniffiMethods.MetadataSborDecode(rawScryptoSbor, networkId);
        return ConvertToolkitMetadataToGateway(metadataValue);
    }

    public static GatewayModel.MetadataTypedValue ConvertToolkitMetadataToGateway(ToolkitModel.MetadataValue metadataValue)
    {
        switch (metadataValue)
        {
            case ToolkitModel.MetadataValue.BoolArrayValue boolArrayValue:
                return new GatewayModel.MetadataBoolArrayValue(boolArrayValue.value.Select(x => x).ToList());
            case ToolkitModel.MetadataValue.BoolValue boolValue:
                return new GatewayModel.MetadataBoolValue(boolValue.value);
            case ToolkitModel.MetadataValue.DecimalArrayValue decimalArrayValue:
                return new GatewayModel.MetadataDecimalArrayValue(decimalArrayValue.value.Select(x => x.AsStr()).ToList());
            case ToolkitModel.MetadataValue.DecimalValue decimalValue:
                return new GatewayModel.MetadataDecimalValue(decimalValue.value.AsStr());
            case ToolkitModel.MetadataValue.GlobalAddressArrayValue globalAddressArrayValue:
                return new GatewayModel.MetadataGlobalAddressArrayValue(globalAddressArrayValue.value.Select(x => x.AddressString()).ToList());
            case ToolkitModel.MetadataValue.GlobalAddressValue globalAddressValue:
                return new GatewayModel.MetadataGlobalAddressValue(globalAddressValue.value.AddressString());
            case ToolkitModel.MetadataValue.I32ArrayValue i32ArrayValue:
                return new GatewayModel.MetadataI32ArrayValue(i32ArrayValue.value.Select(x => x.ToString()).ToList());
            case ToolkitModel.MetadataValue.I32Value i32Value:
                return new GatewayModel.MetadataI32Value(i32Value.value.ToString());
            case ToolkitModel.MetadataValue.I64ArrayValue i64ArrayValue:
                return new GatewayModel.MetadataI64ArrayValue(i64ArrayValue.value.Select(x => x.ToString()).ToList());
            case ToolkitModel.MetadataValue.I64Value i64Value:
                return new GatewayModel.MetadataI64Value(i64Value.value.ToString());
            case ToolkitModel.MetadataValue.InstantArrayValue instantArrayValue:
                return new GatewayModel.MetadataInstantArrayValue(instantArrayValue.value.Select(x => DateTimeOffset.FromUnixTimeSeconds(x).AsUtcIsoDateAtSecondsPrecisionString()).ToList());
            case ToolkitModel.MetadataValue.InstantValue instantValue:
                return new GatewayModel.MetadataInstantValue(DateTimeOffset.FromUnixTimeSeconds(instantValue.value).AsUtcIsoDateAtSecondsPrecisionString());
            case ToolkitModel.MetadataValue.NonFungibleGlobalIdArrayValue nonFungibleGlobalIdArrayValue:
                return new GatewayModel.MetadataNonFungibleGlobalIdArrayValue(nonFungibleGlobalIdArrayValue
                    .value
                    .Select(x => new GatewayModel.MetadataNonFungibleGlobalIdValueAllOf(
                        x.ResourceAddress().AddressString(),
                        ToolkitModel.RadixEngineToolkitUniffiMethods.NonFungibleLocalIdAsStr(x.LocalId())))
                    .ToList());
            case ToolkitModel.MetadataValue.NonFungibleGlobalIdValue nonFungibleGlobalIdValue:
                return new GatewayModel.MetadataNonFungibleGlobalIdValue(
                    nonFungibleGlobalIdValue.value.ResourceAddress().AddressString(),
                    ToolkitModel.RadixEngineToolkitUniffiMethods.NonFungibleLocalIdAsStr(nonFungibleGlobalIdValue.value.LocalId()));
            case ToolkitModel.MetadataValue.NonFungibleLocalIdArrayValue nonFungibleLocalIdArrayValue:
                return new GatewayModel.MetadataNonFungibleLocalIdArrayValue(nonFungibleLocalIdArrayValue.value.Select(ToolkitModel.RadixEngineToolkitUniffiMethods.NonFungibleLocalIdAsStr).ToList());
            case ToolkitModel.MetadataValue.NonFungibleLocalIdValue nonFungibleLocalIdValue:
                return new GatewayModel.MetadataNonFungibleLocalIdValue(ToolkitModel.RadixEngineToolkitUniffiMethods.NonFungibleLocalIdAsStr(nonFungibleLocalIdValue.value));
            case ToolkitModel.MetadataValue.OriginArrayValue originArrayValue:
                return new GatewayModel.MetadataOriginArrayValue(originArrayValue.value.Select(x => x.ToString()).ToList());
            case ToolkitModel.MetadataValue.OriginValue originValue:
                return new GatewayModel.MetadataOriginValue(originValue.value);
            case ToolkitModel.MetadataValue.PublicKeyArrayValue publicKeyArrayValue:
                return new GatewayModel.MetadataPublicKeyArrayValue(publicKeyArrayValue.value.Select(x => x.ToGatewayModel()).ToList());
            case ToolkitModel.MetadataValue.PublicKeyValue publicKeyValue:
                return new GatewayModel.MetadataPublicKeyValue(publicKeyValue.value.ToGatewayModel());
            case ToolkitModel.MetadataValue.PublicKeyHashArrayValue publicKeyHashArray:
                return new GatewayModel.MetadataPublicKeyHashArrayValue(publicKeyHashArray.value.Select(x => x.ToGatewayModel()).ToList());
            case ToolkitModel.MetadataValue.PublicKeyHashValue publicKeyHashValue:
                return new GatewayModel.MetadataPublicKeyHashValue(publicKeyHashValue.value.ToGatewayModel());
            case ToolkitModel.MetadataValue.StringArrayValue stringArrayValue:
                return new GatewayModel.MetadataStringArrayValue(stringArrayValue.value.ToList());
            case ToolkitModel.MetadataValue.StringValue stringValue:
                return new GatewayModel.MetadataStringValue(stringValue.value);
            case ToolkitModel.MetadataValue.U32ArrayValue u32ArrayValue:
                return new GatewayModel.MetadataU32ArrayValue(u32ArrayValue.value.Select(x => x.ToString()).ToList());
            case ToolkitModel.MetadataValue.U32Value u32Value:
                return new GatewayModel.MetadataU32Value(u32Value.value.ToString());
            case ToolkitModel.MetadataValue.U64ArrayValue u64ArrayValue:
                return new GatewayModel.MetadataU64ArrayValue(u64ArrayValue.value.Select(x => x.ToString()).ToList());
            case ToolkitModel.MetadataValue.U64Value u64Value:
                return new GatewayModel.MetadataU64Value(u64Value.value.ToString());
            case ToolkitModel.MetadataValue.U8ArrayValue u8ArrayValue:
                return new GatewayModel.MetadataU8ArrayValue(u8ArrayValue.value.ToArray().ToHex());
            case ToolkitModel.MetadataValue.U8Value u8Value:
                return new GatewayModel.MetadataU8Value(u8Value.value.ToString());
            case ToolkitModel.MetadataValue.UrlArrayValue urlArrayValue:
                return new GatewayModel.MetadataUrlArrayValue(urlArrayValue.value.ToList());
            case ToolkitModel.MetadataValue.UrlValue urlValue:
                return new GatewayModel.MetadataUrlValue(urlValue.value);
            default:
                throw new NotSupportedException($"Unexpected metadataValue type {metadataValue.GetType()}");
        }
    }
}
