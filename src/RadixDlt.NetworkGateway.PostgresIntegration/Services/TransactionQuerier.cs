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
using RadixDlt.NetworkGateway.Abstractions.Extensions;
using RadixDlt.NetworkGateway.Abstractions.Model;
using RadixDlt.NetworkGateway.GatewayApi.Services;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GatewayModel = RadixDlt.NetworkGateway.GatewayApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Services;

internal class TransactionQuerier : ITransactionQuerier
{
    private readonly ReadOnlyDbContext _dbContext;
    private readonly ReadWriteDbContext _rwDbContext;
    private readonly INetworkConfigurationProvider _networkConfigurationProvider;

    public TransactionQuerier(ReadOnlyDbContext dbContext, ReadWriteDbContext rwDbContext, INetworkConfigurationProvider networkConfigurationProvider)
    {
        _dbContext = dbContext;
        _rwDbContext = rwDbContext;
        _networkConfigurationProvider = networkConfigurationProvider;
    }

    public async Task<TransactionPageWithoutTotal> GetRecentUserTransactions(
        RecentTransactionPageRequest request,
        GatewayModel.LedgerState atLedgerState,
        GatewayModel.LedgerState? fromLedgerState,
        CancellationToken token = default)
    {
        var transactionStateVersionsAndOneMore = await GetRecentUserTransactionStateVersions(request, atLedgerState, fromLedgerState, token);
        var nextCursor = transactionStateVersionsAndOneMore.Count == request.PageSize + 1
            ? new GatewayModel.LedgerTransactionsCursor(transactionStateVersionsAndOneMore.Last())
            : null;

        var transactions = await GetTransactions(transactionStateVersionsAndOneMore.Take(request.PageSize).ToList(), token);

        if (fromLedgerState != null)
        {
            transactions.Reverse();
        }

        return new TransactionPageWithoutTotal(nextCursor, transactions);
    }

    public async Task<LookupResult?> LookupCommittedTransaction(GatewayModel.TransactionLookupIdentifier lookup, GatewayModel.LedgerState ledgerState, bool withDetails, CancellationToken token = default)
    {
        var hash = lookup.ValueHex.ConvertFromHex();
        var query = _dbContext.LedgerTransactions
            .OfType<UserLedgerTransaction>()
            .Where(ult => ult.StateVersion <= ledgerState.StateVersion);

        switch (lookup.Origin)
        {
            case GatewayModel.TransactionLookupOrigin.Intent:
                query = query.Where(ult => ult.IntentHash == hash);
                break;
            case GatewayModel.TransactionLookupOrigin.SignedIntent:
                query = query.Where(ult => ult.SignedIntentHash == hash);
                break;
            case GatewayModel.TransactionLookupOrigin.Notarized:
                throw new NotImplementedException(); // TODO see https://rdxworks.slack.com/archives/D03P4L6J0RM/p1668072045704119
            case GatewayModel.TransactionLookupOrigin.Payload:
                query = query.Where(ult => ult.PayloadHash == hash);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(lookup.Origin), lookup.Origin, null);
        }

        var stateVersion = await query
            .Select(ult => ult.StateVersion)
            .FirstOrDefaultAsync(token);

        if (stateVersion == default)
        {
            return null;
        }

        return withDetails
            ? await GetTransactionWithDetails(stateVersion, token)
            : new LookupResult((await GetTransactions(new List<long> { stateVersion }, token)).First(), null);
    }

    public async Task<GatewayModel.TransactionInfo?> LookupPendingTransaction(GatewayModel.TransactionLookupIdentifier lookup, CancellationToken token = default)
    {
        var hash = lookup.ValueHex.ConvertFromHex();
        var query = _rwDbContext.PendingTransactions.AsQueryable();

        switch (lookup.Origin)
        {
            case GatewayModel.TransactionLookupOrigin.Intent:
                query = query.Where(pt => pt.IntentHash == hash);
                break;
            case GatewayModel.TransactionLookupOrigin.SignedIntent:
                query = query.Where(pt => pt.SignedIntentHash == hash);
                break;
            case GatewayModel.TransactionLookupOrigin.Notarized:
                throw new NotImplementedException(); // TODO see https://rdxworks.slack.com/archives/D03P4L6J0RM/p1668072045704119
            case GatewayModel.TransactionLookupOrigin.Payload:
                query = query.Where(pt => pt.PayloadHash == hash);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(lookup.Origin), lookup.Origin, null);
        }

        // We lookup the pending transaction using the _rwDbContext which is bound to the
        // ReadWriteDbContext so that it gets the most recent details -- to ensure that submitted transactions
        // are immediately shown as pending.
        var pendingTransaction = await query
            .OrderByDescending(pt => pt.CommitTimestamp)
            .FirstOrDefaultAsync(token);

        if (pendingTransaction == default)
        {
            return null;
        }

        var status = pendingTransaction.Status switch
        {
            PendingTransactionStatus.Committed => GatewayModel.TransactionStatus.StatusEnum.Succeeded, // TODO we can't assume it has succeeded, see https://github.com/radixdlt/babylon-gateway/pull/64#discussion_r1021967257
            PendingTransactionStatus.SubmittedOrKnownInNodeMempool => GatewayModel.TransactionStatus.StatusEnum.Pending,
            PendingTransactionStatus.Missing => GatewayModel.TransactionStatus.StatusEnum.Pending,
            PendingTransactionStatus.ResolvedButUnknownTillSyncedUp => GatewayModel.TransactionStatus.StatusEnum.Pending,
            PendingTransactionStatus.Failed => GatewayModel.TransactionStatus.StatusEnum.Failed,
            _ => throw new ArgumentOutOfRangeException(),
        };

        return new GatewayModel.TransactionInfo(
            transactionStatus: new GatewayModel.TransactionStatus(status),
            payloadHashHex: pendingTransaction.PayloadHash.ToHex(),
            intentHashHex: pendingTransaction.IntentHash.ToHex(),
            feePaid: null
        );
    }

    private async Task<List<long>> GetRecentUserTransactionStateVersions(
        RecentTransactionPageRequest request,
        GatewayModel.LedgerState atLedgerState,
        GatewayModel.LedgerState? fromLedgerState,
        CancellationToken token)
    {
        if (fromLedgerState != null)
        {
            var bottomStateVersionBoundary = request.Cursor?.StateVersionBoundary ?? fromLedgerState.StateVersion;
            var topStateVersionBoundary = atLedgerState.StateVersion;

            return await _dbContext.LedgerTransactions
                .Where(lt =>
                    lt.StateVersion >= bottomStateVersionBoundary && lt.StateVersion <= topStateVersionBoundary
                    && !lt.IsStartOfEpoch
                    && !lt.IsStartOfRound
                )
                .OrderBy(at => at.StateVersion)
                .Take(request.PageSize + 1)
                .Select(at => at.StateVersion)
                .ToListAsync(token);
        }
        else
        {
            var topStateVersionBoundary = request.Cursor?.StateVersionBoundary ?? atLedgerState.StateVersion;

            return await _dbContext.LedgerTransactions
                .OfType<UserLedgerTransaction>()
                .Where(ult => ult.StateVersion <= topStateVersionBoundary)
                .OrderByDescending(ult => ult.StateVersion)
                .Take(request.PageSize + 1)
                .Select(ult => ult.StateVersion)
                .ToListAsync(token);
        }
    }

    private async Task<List<GatewayModel.TransactionInfo>> GetTransactions(List<long> transactionStateVersions, CancellationToken token)
    {
        var transactions = await _dbContext.LedgerTransactions
            .OfType<UserLedgerTransaction>()
            .Where(ult => transactionStateVersions.Contains(ult.StateVersion))
            .OrderByDescending(ult => ult.StateVersion)
            .ToListAsync(token);

        return transactions.Select(MapToGatewayAccountTransaction).ToList();
    }

    private async Task<LookupResult> GetTransactionWithDetails(long stateVersion, CancellationToken token)
    {
        // TODO how to execute that with join?

        var transaction = await _dbContext.LedgerTransactions
            .OfType<UserLedgerTransaction>()
            .Where(ult => ult.StateVersion == stateVersion)
            .OrderByDescending(lt => lt.StateVersion)
            .FirstAsync(token);

        var rawTransaction = await _dbContext.RawTransactions
            .FirstAsync(rt => rt.StateVersion == transaction.StateVersion, token);

        List<Entity> referencedEntities = new List<Entity>();

        if (transaction.ReferencedEntities.Any())
        {
            referencedEntities = await _dbContext.Entities
                .Where(e => transaction.ReferencedEntities.Contains(e.Id))
                .ToListAsync(token);
        }

        return MapToGatewayAccountTransactionWithDetails(transaction, rawTransaction, referencedEntities);
    }

    private GatewayModel.TransactionInfo MapToGatewayAccountTransaction(UserLedgerTransaction ult)
    {
        var status = ult.Status switch
        {
            LedgerTransactionStatus.Succeeded => GatewayModel.TransactionStatus.StatusEnum.Succeeded,
            LedgerTransactionStatus.Failed => GatewayModel.TransactionStatus.StatusEnum.Failed,
            LedgerTransactionStatus.Rejected => GatewayModel.TransactionStatus.StatusEnum.Rejected,
            _ => throw new ArgumentOutOfRangeException(),
        };

        return new GatewayModel.TransactionInfo(
            transactionStatus: new GatewayModel.TransactionStatus(status, ult.StateVersion, ult.RoundTimestamp),
            payloadHashHex: ult.PayloadHash.ToHex(),
            intentHashHex: ult.IntentHash.ToHex(),
            feePaid: new GatewayModel.TokenAmount(ult.FeePaid.ToString(), _networkConfigurationProvider.GetXrdAddress())
        );
    }

    private LookupResult MapToGatewayAccountTransactionWithDetails(UserLedgerTransaction ult, RawTransaction rawTransaction, List<Entity> referencedEntities)
    {
        return new LookupResult(MapToGatewayAccountTransaction(ult), new GatewayModel.TransactionDetails(
            rawHex: rawTransaction.Payload.ToHex(),
            referencedGlobalEntities: referencedEntities.Where(re => re.GlobalAddress != null).Select(re => re.BuildHrpGlobalAddress(_networkConfigurationProvider.GetHrpDefinition())).ToList(),
            messageHex: ult.Message?.ToHex()
        ));
    }
}
