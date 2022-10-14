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

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RadixDlt.NetworkGateway.Abstractions;
using RadixDlt.NetworkGateway.Abstractions.Extensions;
using RadixDlt.NetworkGateway.GatewayApi.Configuration;
using RadixDlt.NetworkGateway.GatewayApi.Exceptions;
using RadixDlt.NetworkGateway.GatewayApi.Services;
using RadixDlt.NetworkGateway.GatewayApiSdk.Model;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Services;

internal class LedgerStateQuerier : ILedgerStateQuerier
{
    private readonly ILogger<LedgerStateQuerier> _logger;
    private readonly ReadOnlyDbContext _dbContext;
    private readonly INetworkConfigurationProvider _networkConfigurationProvider;
    private readonly IOptionsMonitor<EndpointOptions> _endpointOptionsMonitor;
    private readonly IOptionsMonitor<AcceptableLedgerLagOptions> _acceptableLedgerLagOptionsMonitor;
    private readonly IEnumerable<ILedgerStateQuerierObserver> _observers;
    private readonly IClock _clock;

    public LedgerStateQuerier(
        ILogger<LedgerStateQuerier> logger,
        ReadOnlyDbContext dbContext,
        INetworkConfigurationProvider networkConfigurationProvider,
        IOptionsMonitor<EndpointOptions> endpointOptionsMonitor,
        IOptionsMonitor<AcceptableLedgerLagOptions> acceptableLedgerLagOptionsMonitor,
        IEnumerable<ILedgerStateQuerierObserver> observers,
        IClock clock)
    {
        _logger = logger;
        _dbContext = dbContext;
        _networkConfigurationProvider = networkConfigurationProvider;
        _endpointOptionsMonitor = endpointOptionsMonitor;
        _acceptableLedgerLagOptionsMonitor = acceptableLedgerLagOptionsMonitor;
        _observers = observers;
        _clock = clock;
    }

    public async Task<GatewayResponse> GetGatewayState(CancellationToken token)
    {
        var ledgerStatus = await GetLedgerStatus(token);
        return new GatewayResponse(
            new GatewayApiVersions(
                _endpointOptionsMonitor.CurrentValue.GatewayApiVersion,
                _endpointOptionsMonitor.CurrentValue.GatewayOpenApiSchemaVersion
            ),
            new LedgerState(
                _networkConfigurationProvider.GetNetworkName(),
                ledgerStatus.TopOfLedgerTransaction.StateVersion,
                ledgerStatus.TopOfLedgerTransaction.RoundTimestamp.AsUtcIsoDateWithMillisString(),
                ledgerStatus.TopOfLedgerTransaction.Epoch,
                ledgerStatus.TopOfLedgerTransaction.RoundInEpoch
            ),
            new TargetLedgerState(ledgerStatus.TargetStateVersion)
        );
    }

    // So that we don't forget to check the network name, add the assertion in here.
    public async Task<LedgerState> GetValidLedgerStateForReadRequest(PartialLedgerStateIdentifier? atLedgerStateIdentifier, CancellationToken token = default)
    {
        var ledgerStateReport = await GetLedgerState(atLedgerStateIdentifier, token);
        var ledgerState = ledgerStateReport.LedgerState;

        if (atLedgerStateIdentifier == null)
        {
            return ledgerState;
        }

        var acceptableLedgerLag = _acceptableLedgerLagOptionsMonitor.CurrentValue;
        var timestampDiff = _clock.UtcNow - ledgerStateReport.RoundTimestamp;

        await _observers.ForEachAsync(x => x.LedgerRoundTimestampClockSkew(timestampDiff));

        if (timestampDiff.TotalSeconds <= acceptableLedgerLag.ReadRequestAcceptableDbLedgerLagSeconds)
        {
            return ledgerState;
        }

        if (acceptableLedgerLag.PreventReadRequestsIfDbLedgerIsBehind)
        {
            throw NotSyncedUpException.FromRequest(
                NotSyncedUpRequestType.Read,
                timestampDiff,
                acceptableLedgerLag.ReadRequestAcceptableDbLedgerLagSeconds
            );
        }

        _logger.LogWarning(
            "The DB ledger is currently {HumanReadableDelay} behind, so the read query will not be up to date with the current ledger",
            timestampDiff.FormatPositiveDurationHumanReadable()
        );

        return ledgerState;
    }

    public async Task<LedgerState?> GetValidLedgerStateForReadForwardRequest(PartialLedgerStateIdentifier? fromLedgerStateIdentifier, CancellationToken token = default)
    {
        LedgerStateReport? ledgerStateReport = null;

        if (fromLedgerStateIdentifier?.HasStateVersion == true)
        {
            ledgerStateReport = await GetLedgerStateAfterStateVersion(fromLedgerStateIdentifier.StateVersion.Value, token);
        }
        else if (fromLedgerStateIdentifier?.HasTimestamp == true)
        {
            ledgerStateReport = await GetLedgerStateAfterTimestamp(fromLedgerStateIdentifier.Timestamp.Value, token);
        }
        else if (fromLedgerStateIdentifier?.HasEpoch == true)
        {
            ledgerStateReport = await GetLedgerStateAfterEpochAndRound(fromLedgerStateIdentifier.Epoch.Value, fromLedgerStateIdentifier.Round ?? 0, token);
        }

        return ledgerStateReport?.LedgerState;
    }

    public async Task<LedgerState> GetValidLedgerStateForConstructionRequest(PartialLedgerStateIdentifier? atLedgerStateIdentifier, CancellationToken token = default)
    {
        var ledgerStateReport = await GetLedgerState(atLedgerStateIdentifier, token);
        var ledgerState = ledgerStateReport.LedgerState;

        if (atLedgerStateIdentifier == null)
        {
            return ledgerState;
        }

        var acceptableLedgerLag = _acceptableLedgerLagOptionsMonitor.CurrentValue;
        var timestampDiff = _clock.UtcNow - ledgerStateReport.RoundTimestamp;

        await _observers.ForEachAsync(x => x.LedgerRoundTimestampClockSkew(timestampDiff));

        if (timestampDiff.TotalSeconds <= acceptableLedgerLag.ConstructionRequestsAcceptableDbLedgerLagSeconds)
        {
            return ledgerState;
        }

        if (acceptableLedgerLag.PreventConstructionRequestsIfDbLedgerIsBehind)
        {
            throw NotSyncedUpException.FromRequest(
                NotSyncedUpRequestType.Construction,
                timestampDiff,
                acceptableLedgerLag.ConstructionRequestsAcceptableDbLedgerLagSeconds
            );
        }

        _logger.LogWarning(
            "The DB ledger is currently {HumanReadableDelay} behind, so the construction query may be validated incorrectly (or built incorrectly using historic stake records for unstake calculations)",
            timestampDiff.FormatPositiveDurationHumanReadable()
        );

        return ledgerState;
    }

    public async Task<long> GetTopOfLedgerStateVersion(CancellationToken token = default)
    {
        var ledgerStatus = await GetLedgerStatus(token);

        return ledgerStatus.TopOfLedgerStateVersion;
    }

    private async Task<LedgerStatus> GetLedgerStatus(CancellationToken token)
    {
        var ledgerStatus = await _dbContext.LedgerStatus
            .Include(ls => ls.TopOfLedgerTransaction)
            .SingleOrDefaultAsync(token);

        if (ledgerStatus == null)
        {
            throw new InvalidStateException("There are no transactions in the database");
        }

        return ledgerStatus;
    }

    private record LedgerStateReport(LedgerState LedgerState, DateTimeOffset RoundTimestamp);

    private async Task<LedgerStateReport> GetLedgerState(PartialLedgerStateIdentifier? at = null, CancellationToken token = default)
    {
        LedgerStateReport result;

        if (at?.HasStateVersion == true)
        {
            result = await GetLedgerStateBeforeStateVersion(at.StateVersion.Value, token);
        }
        else if (at?.HasTimestamp == true)
        {
            result = await GetLedgerStateBeforeTimestamp(at.Timestamp.Value, token);
        }
        else if (at?.HasEpoch == true)
        {
            result = await GetLedgerStateAtEpochAndRound(at.Epoch.Value, at.Round ?? 0, token);
        }
        else
        {
            result = await GetTopOfLedgerStateReport(token);
        }

        return result;
    }

    private async Task<LedgerStateReport> GetTopOfLedgerStateReport(CancellationToken token)
    {
        var ledgerState = await GetLedgerStateFromQuery(_dbContext.GetTopLedgerTransaction(), token);

        if (ledgerState == null)
        {
            throw new InvalidStateException("There are no transactions in the database");
        }

        return ledgerState;
    }

    private async Task<LedgerStateReport> GetLedgerStateBeforeStateVersion(long stateVersion, CancellationToken token)
    {
        var ledgerState = await GetLedgerStateFromQuery(_dbContext.GetLatestLedgerTransactionBeforeStateVersion(stateVersion), token);

        if (ledgerState == null)
        {
            throw new InvalidStateException("There are no transactions in the database");
        }

        return ledgerState;
    }

    private async Task<LedgerStateReport> GetLedgerStateAfterStateVersion(long stateVersion, CancellationToken token)
    {
        var ledgerState = await GetLedgerStateFromQuery(_dbContext.GetFirstLedgerTransactionAfterStateVersion(stateVersion), token);

        if (ledgerState == null)
        {
            throw new InvalidStateException("State version is beyond the end of the known ledger");
        }

        return ledgerState;
    }

    private async Task<LedgerStateReport> GetLedgerStateBeforeTimestamp(DateTimeOffset timestamp, CancellationToken token)
    {
        var ledgerState = await GetLedgerStateFromQuery(_dbContext.GetLatestLedgerTransactionBeforeTimestamp(timestamp), token);

        if (ledgerState == null)
        {
            throw InvalidRequestException.FromOtherError("Timestamp was before the start of the ledger");
        }

        return ledgerState;
    }

    private async Task<LedgerStateReport> GetLedgerStateAfterTimestamp(DateTimeOffset timestamp, CancellationToken token)
    {
        var ledgerState = await GetLedgerStateFromQuery(_dbContext.GetFirstLedgerTransactionAfterTimestamp(timestamp), token);

        if (ledgerState == null)
        {
            throw InvalidRequestException.FromOtherError("Timestamp is beyond the end of the known ledger");
        }

        return ledgerState;
    }

    private async Task<LedgerStateReport> GetLedgerStateAtEpochAndRound(long epoch, long round, CancellationToken token)
    {
        var ledgerState = await GetLedgerStateFromQuery(_dbContext.GetLatestLedgerTransactionAtEpochRound(epoch, round), token);

        if (ledgerState == null)
        {
            throw InvalidRequestException.FromOtherError($"Epoch {epoch} is beyond the end of the known ledger");
        }

        return ledgerState;
    }

    private async Task<LedgerStateReport> GetLedgerStateAfterEpochAndRound(long epoch, long round, CancellationToken token)
    {
        var ledgerState = await GetLedgerStateFromQuery(_dbContext.GetFirstLedgerTransactionAtEpochRound(epoch, round), token);

        if (ledgerState == null)
        {
            throw InvalidRequestException.FromOtherError($"Epoch {epoch} round {round} is beyond the end of the known ledger");
        }

        return ledgerState;
    }

    private async Task<LedgerStateReport?> GetLedgerStateFromQuery(IQueryable<LedgerTransaction> query, CancellationToken token)
    {
        var lt = await query
            .Select(lt => new
            {
                lt.StateVersion,
                lt.RoundTimestamp,
                lt.Epoch,
                lt.RoundInEpoch,
            })
            .SingleOrDefaultAsync(token);

        return lt == null ? null : new LedgerStateReport(
            new LedgerState(
                _networkConfigurationProvider.GetNetworkName(),
                lt.StateVersion,
                lt.RoundTimestamp.AsUtcIsoDateWithMillisString(),
                lt.Epoch,
                lt.RoundInEpoch
            ),
            lt.RoundTimestamp
        );
    }
}
