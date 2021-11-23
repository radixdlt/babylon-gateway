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

using Common.Database.Models.Ledger.Normalization;
using Common.Database.ValueConverters;
using Common.Numerics;
using Microsoft.EntityFrameworkCore;
using RadixCoreApi.GeneratedClient.Model;
using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Database.Models.Ledger.Substates;

public enum ResourceDataSubstateType
{
    TokenData,
    TokenMetadata,
}

public class ResourceDataSubstateTypeValueConverter : EnumTypeValueConverterBase<ResourceDataSubstateType>
{
    private static readonly Dictionary<ResourceDataSubstateType, string> _conversion = new()
    {
        { ResourceDataSubstateType.TokenData, "TOKEN_DATA" },
        { ResourceDataSubstateType.TokenMetadata, "TOKEN_METADATA" },
    };

    public ResourceDataSubstateTypeValueConverter()
        : base(_conversion, Invert(_conversion))
    {
    }
}

/// <summary>
/// Combines TokenData and TokenMetadata.
/// </summary>
[Index(nameof(ResourceId))]
[Table("resource_data_substates")]
public class ResourceDataSubstate : DataSubstateBase
{
    [Column(name: "resource_id")]
    public long ResourceId { get; set; }

    [ForeignKey(nameof(ResourceId))]
    public Resource Resource { get; set; }

    [Column(name: "type")]
    public ResourceDataSubstateType Type { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ResourceDataSubstate"/> class.
    /// The SubstateBase properties should be set separately.
    /// </summary>
    public ResourceDataSubstate(Resource resource, ResourceDataSubstateType type, bool? isMutable, TokenAmount? granularity, string? owner, string? symbol, string? name, string? description, string? url, string? iconUrl)
    {
        Resource = resource;
        Type = type;
        IsMutable = isMutable;
        Granularity = granularity;
        Owner = owner;
        Symbol = symbol;
        Name = name;
        Description = description;
        Url = url;
        IconUrl = iconUrl;
    }

    public bool? IsMutable { get; set; }

    public TokenAmount? Granularity { get; set; }

    public string? Owner { get; set; }

    public string? Symbol { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public string? Url { get; set; }

    public string? IconUrl { get; set; }

    private ResourceDataSubstate()
    {
    }
}
