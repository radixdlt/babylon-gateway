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

using FluentValidation;
using Microsoft.Extensions.Configuration;
using RadixDlt.NetworkGateway.Abstractions.Configuration;
using System;

namespace RadixDlt.NetworkGateway.DataAggregator.Configuration;

public sealed record LedgerConfirmationOptions
{
    /// <summary>
    /// Gets or sets MaxCommitBatchSize.
    /// The maximum batch to send to the ledger extension service for committing.
    /// </summary>
    [ConfigurationKeyName("MaxCommitBatchSize")]
    public long MaxCommitBatchSize { get; set; } = 1000;

    /// <summary>
    /// Gets or sets MinCommitBatchSize.
    /// The minimum batch to send to the ledger extension service for committing.
    /// It was added to have more control over batch size in tests.
    /// We expect that best value to be used on production network is 1.
    /// </summary>
    [ConfigurationKeyName("MinCommitBatchSize")]
    public long MinCommitBatchSize { get; set; } = 1;

    /// <summary>
    /// Gets or sets LargeBatchSizeToAddDelay.
    /// LargeBatchSizeToAddDelay determines if the DelayBetweenLargeBatchesMilliseconds should be added.
    /// This is only relevant if a DelayBetweenLargeBatchesMilliseconds is configured.
    /// This property essentially determines if the syncing is in large batches (catch-up syncing) or small batches
    /// (likely realtime syncing, where resource use isn't full and adding a delay isn't so necessary).
    /// The delay is only added when the ingested batch size exceeds this threshold. This should be set to be less than
    /// or equal to MaxCommitBatchSize.
    /// </summary>
    [ConfigurationKeyName("LargeBatchSizeToAddDelay")]
    public long LargeBatchSizeToAddDelay { get; set; } = 500;

    /// <summary>
    /// Gets or sets DelayBetweenLargeBatchesMilliseconds.
    /// This delay allows for a simple means to limit the resource usage of the Gateway / DB during syncing.
    /// See also LargeBatchSizeToAddDelay.
    /// </summary>
    [ConfigurationKeyName("DelayBetweenLargeBatchesMilliseconds")]
    public long DelayBetweenLargeBatchesMilliseconds { get; set; } = 0;

    public TimeSpan DelayBetweenLargeBatches => TimeSpan.FromMilliseconds(DelayBetweenLargeBatchesMilliseconds);

    /// <summary>
    /// Gets or sets MaxTransactionPipelineSizePerNode.
    /// This allows this many transactions to be stored ahead of the committed ledger height for each node, to speed
    /// up ingestion.
    /// </summary>
    [ConfigurationKeyName("MaxTransactionPipelineSizePerNode")]
    public long MaxTransactionPipelineSizePerNode { get; set; } = 3000;

    /// <summary>
    /// Gets or sets MaxEstimatedTransactionPipelineByteSizePerNode.
    /// This allows roughly this many bytes to be consumed form upstream API to avoid memory starvation issues.
    /// </summary>
    /// <remarks>
    /// This constraint works in conjunction with <see cref="MaxTransactionPipelineSizePerNode"/> where it should
    /// be considered a secondary fail-safe mechanism that should prevent rare, but not impossible situation where
    /// ingested transaction stream consists of extremely large transactions.
    /// </remarks>
    [ConfigurationKeyName("MaxEstimatedTransactionPipelineByteSizePerNode")]
    public long MaxEstimatedTransactionPipelineByteSizePerNode { get; set; } = 50 * 1024 * 1024;

    /// <summary>
    /// Gets or sets MaxCoreApiTransactionBatchSize.
    /// This allows to configure how many transactions are fetched from CoreAPI in one batch.
    /// </summary>
    [ConfigurationKeyName("MaxCoreApiTransactionBatchSize")]
    public int MaxCoreApiTransactionBatchSize { get; set; } = 1000;
}

internal class LedgerConfirmationOptionsValidator : AbstractOptionsValidator<LedgerConfirmationOptions>
{
    public LedgerConfirmationOptionsValidator()
    {
        RuleFor(x => x.MaxCommitBatchSize).GreaterThan(0);
        RuleFor(x => x.MinCommitBatchSize).GreaterThan(0);
        RuleFor(x => x.MinCommitBatchSize)
            .LessThanOrEqualTo(x => x.MaxCommitBatchSize)
            .WithMessage(x => $"MinCommitBatchSize is set to: {x.MinCommitBatchSize} but must be lower or equal to MaxCommitBatchSize: {x.MaxCommitBatchSize}");
        RuleFor(x => x.MinCommitBatchSize)
            .LessThanOrEqualTo(x => x.MaxTransactionPipelineSizePerNode)
            .WithMessage(x => $"MinCommitBatchSize is set to: {x.MinCommitBatchSize} but must be lower or equal to MaxTransactionPipelineSizePerNode: {x.MaxTransactionPipelineSizePerNode}");
        RuleFor(x => x.LargeBatchSizeToAddDelay).GreaterThan(0);
        RuleFor(x => x.MaxTransactionPipelineSizePerNode).GreaterThan(0);
        RuleFor(x => x.MaxCoreApiTransactionBatchSize).GreaterThan(0);
    }
}
