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
using Newtonsoft.Json;
using Npgsql;
using RadixDlt.CoreApiSdk.Api;
using RadixDlt.NetworkGateway.Abstractions;
using RadixDlt.NetworkGateway.Abstractions.Configuration;
using RadixDlt.NetworkGateway.Abstractions.CoreCommunications;
using RadixDlt.NetworkGateway.Abstractions.Extensions;
using RadixDlt.NetworkGateway.Abstractions.Model;
using RadixDlt.NetworkGateway.GatewayApi.Services;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoreModel = RadixDlt.CoreApiSdk.Model;
using ToolkitModel = RadixEngineToolkit;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Services;

internal class SubmissionTrackingService : ISubmissionTrackingService
{
    private readonly ReadWriteDbContext _dbContext;
    private readonly ILogger<SubmissionTrackingService> _logger;
    private readonly IReadOnlyCollection<ISubmissionTrackingServiceObserver> _observers;
    private readonly IClock _clock;

    public SubmissionTrackingService(
        ReadWriteDbContext dbContext,
        IEnumerable<ISubmissionTrackingServiceObserver> observers,
        IClock clock,
        ILogger<SubmissionTrackingService> logger)
    {
        _dbContext = dbContext;
        _observers = observers.ToArray();
        _clock = clock;
        _logger = logger;
    }

    public async Task<SubmissionResult> ObserveSubmissionToGatewayAndSubmitToNetworkIfNew(
        TransactionApi transactionApi,
        string networkName,
        string nodeName,
        PendingTransactionHandlingConfig handlingConfig,
        ToolkitModel.NotarizedTransaction notarizedTransaction,
        byte[] notarizedTransactionBytes,
        TimeSpan submissionTimeout,
        long currentEpoch,
        CancellationToken token = default)
    {
        var (alreadyKnown, pendingTransaction) = await HandleObservedSubmission(
            handlingConfig,
            _clock.UtcNow,
            notarizedTransaction,
            notarizedTransactionBytes,
            token);

        if (!alreadyKnown)
        {
            await SubmitToNetworkAndUpdatePendingTransaction(
                new SubmitContext(
                    TransactionApi: transactionApi,
                    TargetNode: nodeName,
                    NetworkName: networkName,
                    SubmissionTimeout: submissionTimeout,
                    IsResubmission: false,
                    ForceNodeToRecalculateResult: false),
                nodeName,
                handlingConfig,
                pendingTransaction,
                notarizedTransactionBytes,
                currentEpoch,
                token);
        }

        if (pendingTransaction.LedgerDetails.PayloadLedgerStatus is PendingTransactionPayloadLedgerStatus.PermanentlyRejected)
        {
            return new SubmissionResult(AlreadyKnown: alreadyKnown, PermanentlyRejectedReason: pendingTransaction.LedgerDetails.LatestRejectionReason);
        }

        return new SubmissionResult(AlreadyKnown: alreadyKnown);
    }

    private async Task<TrackedSubmission> HandleObservedSubmission(
        PendingTransactionHandlingConfig handlingConfig,
        DateTime submittedTimestamp,
        ToolkitModel.NotarizedTransaction notarizedTransaction,
        byte[] notarizedTransactionBytes,
        CancellationToken token = default
    )
    {
        var result = await TrackSubmission(handlingConfig, submittedTimestamp, notarizedTransaction, notarizedTransactionBytes, token);
        await _observers.ForEachAsync(observer => observer.OnSubmissionTrackedInDatabase(result.AlreadyKnown));
        return result;
    }

    private record TrackedSubmission(bool AlreadyKnown, PendingTransaction PendingTransaction);

    private async Task<TrackedSubmission> TrackSubmission(
        PendingTransactionHandlingConfig handlingConfig,
        DateTime submittedTimestamp,
        ToolkitModel.NotarizedTransaction notarizedTransaction,
        byte[] notarizedTransactionBytes,
        CancellationToken token = default
    )
    {
        var payloadHash = notarizedTransaction.NotarizedTransactionHash().AsStr();

        var existingPendingTransaction = await _dbContext
            .PendingTransactions
            .Where(t => t.PayloadHash == payloadHash)
            .AnnotateMetricName()
            .SingleOrDefaultAsync(token);

        if (existingPendingTransaction != null)
        {
            return new TrackedSubmission(true, existingPendingTransaction);
        }

        _logger.LogInformation("[Before] TrackSubmission: intentHash: {transactionHash}, payloadHash: {payloadHash}", notarizedTransaction.IntentHash().AsStr(), payloadHash);

        var pendingTransaction = PendingTransaction.NewAsSubmittedForFirstTimeToGateway(
            handlingConfig,
            payloadHash,
            notarizedTransaction.IntentHash().AsStr(),
            notarizedTransaction.SignedIntent().Intent().Header().endEpochExclusive,
            notarizedTransactionBytes,
            submittedTimestamp
        );

        _dbContext.PendingTransactions.Add(pendingTransaction);

        // We now try saving to the DB - but catch duplicates - if we get a duplicate reported, the gateway which saved
        // it successfully should then submit it to the node -- and the one that reports a duplicate should return a
        // generic success, but not resubmit.
        // NB - 23 is Integrity Constraint Violation: https://www.postgresql.org/docs/current/errcodes-appendix.html)
        try
        {
            await _dbContext.SaveChangesAsync(token);
            _logger.LogInformation(
                "[After] TrackSubmission: id: {transactionHash}, intentHash: {transactionHash}, payloadHash: {payloadHash}",
                pendingTransaction.Id, notarizedTransaction.IntentHash().AsStr(), payloadHash
            );

            return new TrackedSubmission(false, pendingTransaction);
        }
        catch (DbUpdateException ex) when ((ex.InnerException is PostgresException pg) && pg.SqlState.StartsWith("23"))
        {
            return new TrackedSubmission(true, pendingTransaction);
        }
    }

    private async Task SubmitToNetworkAndUpdatePendingTransaction(
        SubmitContext submitContext,
        string nodeName,
        PendingTransactionHandlingConfig handlingConfig,
        PendingTransaction pendingTransaction,
        byte[] notarizedTransactionBytes,
        long currentEpoch,
        CancellationToken token
    )
    {
        try
        {
            var nodeSubmissionResult = await TransactionSubmitter.Submit(
                submitContext,
                notarizedTransactionBytes,
                _observers,
                token
            );

            _logger.LogInformation("[before] HandleNodeSubmissionResult: id: {transactionId}, transaction: {transaction}", pendingTransaction.Id,
                JsonConvert.SerializeObject(pendingTransaction, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));

            pendingTransaction.HandleNodeSubmissionResult(
                handlingConfig,
                nodeName,
                nodeSubmissionResult,
                _clock.UtcNow,
                currentEpoch < 0 ? null : (ulong)currentEpoch);

            await _dbContext.SaveChangesAsync(token);

            _logger.LogInformation("[After] HandleNodeSubmissionResult: id: {transactionId}, transaction: {transaction}", pendingTransaction.Id,
                JsonConvert.SerializeObject(pendingTransaction, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogCritical("DbUpdateConcurrencyException, full serialized exception {exception}", new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

            if (ex.Entries.Any() != true)
            {
                _logger.LogCritical("DbUpdateConcurrencyException with no entries");
            }

            foreach (var entry in ex.Entries)
            {
                if (entry.Entity is PendingTransaction)
                {
                    var proposedValues = entry.CurrentValues;
                    var databaseValues = await entry.GetDatabaseValuesAsync(token);

                    _logger.LogCritical(
                        ex,
                        "Gateway failed to store submission result. PendingTx attempted to save: {toSave} PendingTx fetched from db: {fromDb}, proposedValues: {proposedValues}, currentValues: {dbValues}",
                        JsonConvert.SerializeObject(proposedValues.ToObject(), new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }),
                        JsonConvert.SerializeObject(databaseValues!.ToObject(), new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }),
                        JsonConvert.SerializeObject(proposedValues, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }),
                        JsonConvert.SerializeObject(databaseValues, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore })
                    );
                }
                else
                {
                    _logger.LogCritical(
                        "DbUpdateConcurrencyException has entries but of different type: {type}, {entity}",
                        entry.Entity.GetType(),
                        JsonConvert.SerializeObject(entry.Entity, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore })
                        );
                }
            }
        }
    }
}
