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
using DataAggregator.GlobalServices;
using DataAggregator.LedgerExtension;
using Prometheus;

namespace DataAggregator.Monitoring;

public interface ISystemStatusService
{
    void RecordTransactionsCommitted(CommitTransactionsReport committedTransactionReport);

    void RecordTopOfLedger(TransactionSummary topOfLedger);

    bool IsPrimary();

    HealthReport GenerateHealthReport();
}

// ReSharper disable NotAccessedPositionalProperty.Global - Because they're used in the health response
public record HealthReport(bool IsHealthy, string Reason, DateTimeOffset StartUpTime);

public class SystemStatusService : ISystemStatusService
{
    private static readonly DateTimeOffset _startupTime = DateTimeOffset.UtcNow;
    private static readonly bool _isPrimary = true;

    private static readonly Counter _committedTransactions = Metrics
        .CreateCounter("ledger_committed_transactions_total", "Number of committed transactions.");

    private static readonly Gauge _ledgerLastCommitTimestamp = Metrics
        .CreateGauge("ledger_last_commit_timestamp_seconds", "Number of seconds the DB ledger is behind the present.");

    private static readonly Gauge _ledgerStateVersion = Metrics
        .CreateGauge("ledger_tip_state_version", "The state version of the top of the DB ledger.");

    private static readonly Gauge _ledgerUnixTimestamp = Metrics
        .CreateGauge("ledger_tip_unix_timestamp_seconds", "Unix timestamp of the top of the committed DB ledger.");

    private static readonly Gauge _ledgerSecondsBehind = Metrics
        .CreateGauge("ledger_tip_behind_at_last_commit_seconds", "Number of seconds the DB ledger was behind the present time (at the last time transactions were committed).");

    private readonly IConfiguration _configuration;

    private DateTimeOffset? _lastTransactionCommitment;

    private TimeSpan StartupGracePeriod => TimeSpan.FromSeconds(_configuration.GetSection("Monitoring").GetValue<int?>("StartupGracePeriodSeconds") ?? 10);

    private TimeSpan UnhealthyCommitmentGapSeconds => TimeSpan.FromSeconds(_configuration.GetSection("Monitoring").GetValue<int?>("UnhealthyCommitmentGapSeconds") ?? 20);

    public SystemStatusService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void RecordTransactionsCommitted(CommitTransactionsReport committedTransactionReport)
    {
        _lastTransactionCommitment = DateTimeOffset.UtcNow;
        _committedTransactions.Inc(committedTransactionReport.TransactionsCommittedCount);
        RecordTopOfLedger(committedTransactionReport.FinalTransaction);
    }

    public void RecordTopOfLedger(TransactionSummary topOfLedger)
    {
        _ledgerStateVersion.Set(topOfLedger.StateVersion);
        _ledgerUnixTimestamp.Set(topOfLedger.NormalizedTimestamp.GetUnixTimestampSeconds());
        _ledgerSecondsBehind.Set(topOfLedger.NormalizedTimestamp.GetTimeAgo().TotalSeconds);
    }

    public bool IsPrimary()
    {
        return _isPrimary;
    }

    public HealthReport GenerateHealthReport()
    {
        if (InStartupGracePeriod())
        {
            return new HealthReport(
                true,
                $"Within start up grace period of {StartupGracePeriod}",
                _startupTime
            );
        }

        if (CommittedRecently())
        {
            return new HealthReport(
                true,
                $"Last committed {_lastTransactionCommitment.FormatSecondsAgo()} within healthy period of {UnhealthyCommitmentGapSeconds.FormatSecondsHumanReadable()}",
                _startupTime
            );
        }

        return new HealthReport(
            false,
            $"Last committed {_lastTransactionCommitment.FormatSecondsAgo()}, not within healthy period of {UnhealthyCommitmentGapSeconds.FormatSecondsHumanReadable()}",
            _startupTime
        );
    }

    private bool CommittedRecently()
    {
        return _lastTransactionCommitment != null && _lastTransactionCommitment.Value.WithinPeriodOfNow(UnhealthyCommitmentGapSeconds);
    }

    private bool InStartupGracePeriod()
    {
        return _startupTime.WithinPeriodOfNow(StartupGracePeriod);
    }
}
