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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GatewayModel = RadixDlt.NetworkGateway.GatewayApiSdk.Model;
using ToolkitModel = RadixDlt.RadixEngineToolkit.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration;

internal static class ScryptoSborUtils
{
    public static string ConvertFromScryptoSborString(string input, byte networkId)
    {
        var result = RadixEngineToolkit.RadixEngineToolkit.SborDecode(Convert.FromHexString(input), networkId);

        if (result is not ToolkitModel.Exchange.SborDecodeResponse.ScryptoSbor scryptoSbor)
        {
            throw new UnreachableException("Expected ScryptoSbor response");
        }

        if (scryptoSbor.Value is not ToolkitModel.Value.ScryptoSbor.String value)
        {
            throw new UnreachableException("Expected ScryptoSbor.String");
        }

        return value;
    }

    public static GatewayModel.EntityMetadataItemValue MetadataValueToGatewayMetadataItemValue(byte[] rawScryptoSbor, byte networkId)
    {
        var result = RadixEngineToolkit.RadixEngineToolkit.SborDecode(rawScryptoSbor, networkId);

        if (result is not ToolkitModel.Exchange.SborDecodeResponse.ScryptoSbor scryptoSbor)
        {
            throw new UnreachableException("Expected ScryptoSbor response");
        }

        if (scryptoSbor.Value is not ToolkitModel.Value.ScryptoSbor.Enum outerEnum)
        {
            throw new UnreachableException("Expected ScryptoSbor.Enum");
        }

        string? asString = null;
        List<string>? asStringCollection = null;

        switch (outerEnum.Variant)
        {
            case 0:
            {
                if (outerEnum.Fields is not [ToolkitModel.Value.ScryptoSbor.Enum innerEnum])
                {
                    throw new UnreachableException("Expected ScryptoSbor.Enum");
                }

                if (innerEnum.Variant != 0 || innerEnum.Fields is not [ToolkitModel.Value.ScryptoSbor.String value])
                {
                    throw new UnreachableException("Expected ScryptoSbor.Enum with String-only fields");
                }

                asString = value.Value;

                break;
            }

            case 1:
            {
                if (outerEnum.Fields == null || outerEnum.Fields.Any(f => f is not ToolkitModel.Value.ScryptoSbor.Enum))
                {
                    throw new UnreachableException("Expected ScryptoSbor.Enum with Enum-only fields");
                }

                asStringCollection = new List<string>(outerEnum.Fields.Length);

                foreach (var innerEnum in outerEnum.Fields.Cast<ToolkitModel.Value.ScryptoSbor.Enum>())
                {
                    if (innerEnum.Variant != 0 || innerEnum.Fields is not [ToolkitModel.Value.ScryptoSbor.String value])
                    {
                        throw new UnreachableException("Expected ScryptoSbor.Enum with String-only fields");
                    }

                    asStringCollection.Add(value.Value);
                }

                break;
            }
        }

        return new GatewayModel.EntityMetadataItemValue(
            rawHex: rawScryptoSbor.ToHex(),
            rawJson: new JRaw(RadixEngineToolkit.RadixEngineToolkit.ScryptoSborEncodeJson(outerEnum)),
            asString: asString,
            asStringCollection: asStringCollection);
    }

    public static GatewayModel.ScryptoSborValue NonFungibleDataToGatewayScryptoSbor(byte[] rawScryptoSbor, byte networkId)
    {
        var result = RadixEngineToolkit.RadixEngineToolkit.SborDecode(rawScryptoSbor, networkId);

        if (result is not ToolkitModel.Exchange.SborDecodeResponse.ScryptoSbor scryptoSbor)
        {
            throw new UnreachableException("Expected ScryptoSbor response");
        }

        return new GatewayModel.ScryptoSborValue(
            rawHex: rawScryptoSbor.ToHex(),
            rawJson: new JRaw(RadixEngineToolkit.RadixEngineToolkit.ScryptoSborEncodeJson(scryptoSbor.Value)));
    }
}
