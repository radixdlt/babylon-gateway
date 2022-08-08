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
using NodaTime;
using RadixDlt.NetworkGateway.Core.Configuration;

namespace RadixDlt.NetworkGateway.DataAggregator.Configuration;

public record MempoolOptions
{
    // If enabling this option, you should note the following:
    //   Transactions not submitted by this gateway will never be marked as failed
    //     this is because we're unsure if they might be valid again and someone might resubmit them
    //   Instead, they stick around as PENDING until eventually being pruned
    //     after PruneMissingTransactionsAfterTimeSinceFirstSeenSeconds
    [ConfigurationKeyName("TrackTransactionsNotSubmittedByThisGateway")]
    public bool TrackTransactionsNotSubmittedByThisGateway { get; set; } = false;

    [ConfigurationKeyName("FetchUnknownTransactionFromMempoolDegreeOfParallelizationPerNode")]
    public int FetchUnknownTransactionFromMempoolDegreeOfParallelizationPerNode { get; set; } = 5;

    [ConfigurationKeyName("RecentFetchedUnknownTransactionsCacheSize")]
    public int RecentFetchedUnknownTransactionsCacheSize { get; set; } = 2000;

    [ConfigurationKeyName("ExcludeNodeMempoolsFromUnionIfStaleForSeconds")]
    public long ExcludeNodeMempoolsFromUnionIfStaleForSeconds { get; set; } = 10;

    public Duration ExcludeNodeMempoolsFromUnionIfStaleFor => Duration.FromSeconds(ExcludeNodeMempoolsFromUnionIfStaleForSeconds);

    // This should be above ExcludeNodeMempoolsFromUnionIfStaleFor ideally, as it provides defense in depth against
    // adding a mempool transaction back to the database if a node crashes and so its information is stale
    [ConfigurationKeyName("PruneCommittedAfterSeconds")]
    public long PruneCommittedAfterSeconds { get; set; } = 20;

    public Duration PruneCommittedAfter => Duration.FromSeconds(PruneCommittedAfterSeconds);

    // This is designed to give time for:
    // * The request to be sent (ie the timeout on the submission request should be less than this)
    // * The MempoolTracker to start seeing the transaction in its mempools (if the mempool is backed up) - as it can
    //   read in stale data from nodes.
    // This value won't have any safety implications, but may improve monitoring / db churn.
    // After being marked missing, we still need to wait MinDelayBetweenMissingFromMempoolAndResubmissionSeconds before
    // we can resubmit.
    [ConfigurationKeyName("PostSubmissionGracePeriodBeforeCanBeMarkedMissingMilliseconds")]
    public long PostSubmissionGracePeriodBeforeCanBeMarkedMissingMilliseconds { get; set; } = 5000;

    public Duration PostSubmissionGracePeriodBeforeCanBeMarkedMissing => Duration.FromMilliseconds(PostSubmissionGracePeriodBeforeCanBeMarkedMissingMilliseconds);

    [ConfigurationKeyName("ResubmissionNodeRequestTimeoutMilliseconds")]
    public long ResubmissionNodeRequestTimeoutMilliseconds { get; set; } = 4000;

    public Duration ResubmissionNodeRequestTimeout => Duration.FromMilliseconds(ResubmissionNodeRequestTimeoutMilliseconds);

    [ConfigurationKeyName("AssumedBoundOnNetworkLedgerDataAggregatorClockDriftMilliseconds")]
    public long AssumedBoundOnNetworkLedgerDataAggregatorClockDriftMilliseconds { get; set; } = 1000;

    public Duration AssumedBoundOnNetworkLedgerDataAggregatorClockDrift => Duration.FromMilliseconds(AssumedBoundOnNetworkLedgerDataAggregatorClockDriftMilliseconds);

    [ConfigurationKeyName("MinDelayBetweenResubmissionsSeconds")]
    public long MinDelayBetweenResubmissionsSeconds { get; set; } = 10;

    public Duration MinDelayBetweenResubmissions => Duration.FromSeconds(MinDelayBetweenResubmissionsSeconds);

    // NB - A transaction goes missing from the mempool when it gets put onto the ledger, but it may take some time
    // for the aggregator to see the committed transaction. This delay should be long enough that, under normal
    // operation, we'll have seen the transaction committed before we attempt to resubmit it -- as resubmitting a
    // committed transaction may result in us seeing a self spend, and we're willing to sacrifice some delay on
    // resubmission to likely ideally avoid getting a ResolvedButUnknownTillSyncedUp state in most cases.
    [ConfigurationKeyName("MinDelayBetweenMissingFromMempoolAndResubmissionSeconds")]
    public long MinDelayBetweenMissingFromMempoolAndResubmissionSeconds { get; set; } = 10;

    public Duration MinDelayBetweenMissingFromMempoolAndResubmission => Duration.FromSeconds(MinDelayBetweenMissingFromMempoolAndResubmissionSeconds);

    [ConfigurationKeyName("StopResubmittingAfterSeconds")]
    public long StopResubmittingAfterSeconds { get; set; } = 5 * 60;

    public Duration StopResubmittingAfter => Duration.FromSeconds(StopResubmittingAfterSeconds);

    [ConfigurationKeyName("PruneMissingTransactionsAfterTimeSinceLastGatewaySubmissionSeconds")]
    public long PruneMissingTransactionsAfterTimeSinceLastGatewaySubmissionSeconds { get; set; } = 7 * 24 * 60 * 60; // 1 week

    public Duration PruneMissingTransactionsAfterTimeSinceLastGatewaySubmission => Duration.FromSeconds(PruneMissingTransactionsAfterTimeSinceLastGatewaySubmissionSeconds);

    [ConfigurationKeyName("PruneMissingTransactionsAfterTimeSinceFirstSeenSeconds")]
    public long PruneMissingTransactionsAfterTimeSinceFirstSeenSeconds { get; set; } = 7 * 24 * 60 * 60; // 1 week

    public Duration PruneMissingTransactionsAfterTimeSinceFirstSeen => Duration.FromSeconds(PruneMissingTransactionsAfterTimeSinceFirstSeenSeconds);

    [ConfigurationKeyName("PruneRequiresMissingFromMempoolForSeconds")]
    public long PruneRequiresMissingFromMempoolForSeconds { get; set; } = 60;

    public Duration PruneRequiresMissingFromMempoolFor => Duration.FromSeconds(PruneRequiresMissingFromMempoolForSeconds);
}

internal class MempoolOptionsValidator : AbstractOptionsValidator<MempoolOptions>
{
    public MempoolOptionsValidator()
    {
        RuleFor(x => x.FetchUnknownTransactionFromMempoolDegreeOfParallelizationPerNode).GreaterThan(0);
        RuleFor(x => x.RecentFetchedUnknownTransactionsCacheSize).GreaterThan(0);
        RuleFor(x => x.ExcludeNodeMempoolsFromUnionIfStaleForSeconds).GreaterThan(0);
        RuleFor(x => x.PruneCommittedAfterSeconds).GreaterThan(0);
        RuleFor(x => x.PostSubmissionGracePeriodBeforeCanBeMarkedMissingMilliseconds).GreaterThan(0);
        RuleFor(x => x.ResubmissionNodeRequestTimeoutMilliseconds).GreaterThan(0);
        RuleFor(x => x.AssumedBoundOnNetworkLedgerDataAggregatorClockDriftMilliseconds).GreaterThan(0);
        RuleFor(x => x.MinDelayBetweenResubmissionsSeconds).GreaterThan(0);
        RuleFor(x => x.MinDelayBetweenMissingFromMempoolAndResubmissionSeconds).GreaterThan(0);
        RuleFor(x => x.StopResubmittingAfterSeconds).GreaterThan(0);
        RuleFor(x => x.PruneMissingTransactionsAfterTimeSinceLastGatewaySubmissionSeconds).GreaterThan(0);
        RuleFor(x => x.PruneMissingTransactionsAfterTimeSinceFirstSeenSeconds).GreaterThan(0);
        RuleFor(x => x.PruneRequiresMissingFromMempoolForSeconds).GreaterThan(0);
    }
}
