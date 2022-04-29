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

using Common.Extensions;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Core = RadixCoreApi.Generated.Model;
using Validator = Common.Database.Models.Ledger.Normalization.Validator;

namespace Common.Database.Models.Ledger.Substates;

/// <summary>
/// Represents Validator fork votes.
/// </summary>
[Index(nameof(ValidatorId))]
[Table("validator_system_metadata_substates")]
public class ValidatorSystemMetadataSubstate : DataSubstateBase
{
    [Column(name: "validator_id")]
    public long ValidatorId { get; set; }

    [ForeignKey(nameof(ValidatorId))]
    public Validator Validator { get; set; }

    // This is [Owned] below
    public ValidatorCandidateForkVote? ValidatorCandidateForkVote { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidatorSystemMetadataSubstate"/> class.
    /// The SubstateBase properties should be set separately.
    /// </summary>
    public ValidatorSystemMetadataSubstate(Validator validator, ValidatorCandidateForkVote validatorCandidateForkVote)
    {
        Validator = validator;
        ValidatorCandidateForkVote = validatorCandidateForkVote;
    }

    private ValidatorSystemMetadataSubstate()
    {
    }

    public bool SubstateMatches(ValidatorSystemMetadataSubstate otherSubstate)
    {
        return Validator == otherSubstate.Validator
               && ValidatorCandidateForkVote == otherSubstate.ValidatorCandidateForkVote;
    }
}

[Owned]
// Aka ValidatorSystemMetadata in the engine
public record ValidatorCandidateForkVote
{
    /// <summary>
    ///  The FullBytes field is technically schema-less according to the engine, but a 32 byte vote will be
    ///  interpreted as a vote from that validator for the given fork.
    ///  We attempt to parse out the meaning from the FullBytes field into the other fields.
    /// </summary>
    [Column(name: "full_bytes")]
    public byte[] FullBytes { get; set; }

    [Column(name: "fork_name")]
    public string? ForkName { get; set; }

    [Column(name: "fork_id")]
    public byte[]? ForkId { get; set; }

    [Column(name: "nonce_hash")]
    public byte[]? NonceHash { get; set; }

    public static ValidatorCandidateForkVote From(Core.ValidatorSystemMetadata apiModel)
    {
        var fullBytes = apiModel.Data.ConvertFromHex();
        var fullBytesAreCorrectLength = fullBytes.Length == 32;

        if (!fullBytesAreCorrectLength)
        {
            return new ValidatorCandidateForkVote { FullBytes = fullBytes };
        }

        if (fullBytes.All(b => b == 0))
        {
            // Empty / withdrawn vote - don't extract name etc as they're meaningless
            return new ValidatorCandidateForkVote { FullBytes = fullBytes };
        }

        // See CandidateForkVote in the Java Repo
        var forkName = ExtractForkName(fullBytes[..16]);
        var forkId = fullBytes[..24];
        var nonceHash = fullBytes[24..32];

        return new ValidatorCandidateForkVote
        {
            FullBytes = fullBytes,
            ForkName = forkName,
            ForkId = forkId,
            NonceHash = nonceHash,
        };
    }

    private static string? ExtractForkName(byte[] nameBytes)
    {
        var trimmedNameBytes = nameBytes.Reverse().SkipWhile(b => b == 0).Reverse().ToArray();
        try
        {
            return Encoding.ASCII.GetString(trimmedNameBytes);
        }
        catch (ArgumentException)
        {
            // The byte array contains invalid code points.
            // See: https://docs.microsoft.com/en-us/dotnet/api/System.Text.Encoding.GetString?view=net-6.0
            return null;
        }
    }
}
