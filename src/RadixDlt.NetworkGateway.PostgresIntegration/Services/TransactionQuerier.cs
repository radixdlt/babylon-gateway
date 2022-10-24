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
using RadixDlt.NetworkGateway.GatewayApi;
using RadixDlt.NetworkGateway.GatewayApi.Services;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Gateway = RadixDlt.NetworkGateway.GatewayApiSdk.Model;
using TokenAmount = RadixDlt.NetworkGateway.Abstractions.Numerics.TokenAmount;

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
        Gateway.LedgerState atLedgerState,
        Gateway.LedgerState? fromLedgerState,
        CancellationToken token = default)
    {
        var transactionStateVersionsAndOneMore = await GetRecentUserTransactionStateVersions(request, atLedgerState, fromLedgerState, token);
        var nextCursor = transactionStateVersionsAndOneMore.Count == request.PageSize + 1
            ? new CommittedTransactionPaginationCursor(transactionStateVersionsAndOneMore.Last())
            : null;

        var transactions = await GetTransactions(transactionStateVersionsAndOneMore.Take(request.PageSize).ToList(), token);

        if (fromLedgerState != null)
        {
            transactions.Reverse();
        }

        return new TransactionPageWithoutTotal(nextCursor, transactions);
    }

    public async Task<LookupResult?> LookupCommittedTransaction(
        Gateway.TransactionLookupIdentifier lookup,
        Gateway.LedgerState ledgerState,
        bool withDetails,
        CancellationToken token = default)
    {
        var hash = lookup.ValueHex.ConvertFromHex();
        var query = _dbContext.LedgerTransactions.Where(lt => lt.StateVersion <= ledgerState._Version);

        switch (lookup.Origin)
        {
            case Gateway.TransactionLookupOrigin.Intent:
                query = query.Where(lt => lt.IntentHash == hash);
                break;
            case Gateway.TransactionLookupOrigin.SignedIntent:
                query = query.Where(lt => lt.SignedIntentHash == hash); // TODO fix me
                break;
            case Gateway.TransactionLookupOrigin.Notarized:
                throw new NotImplementedException("fix me"); // TODO fix me
            case Gateway.TransactionLookupOrigin.Payload:
                query = query.Where(lt => lt.PayloadHash == hash);
                break;
            default:
                throw new ArgumentOutOfRangeException("fix me"); // TODO fix me
        }

        var stateVersion = await query
            .Select(lt => lt.StateVersion)
            .SingleOrDefaultAsync(token);

        if (stateVersion == 0)
        {
            return null;
        }

        return withDetails
            ? await GetTransactionWithDetails(stateVersion, token)
            : new LookupResult((await GetTransactions(new List<long> { stateVersion }, token)).First(), null);
    }

    public async Task<Gateway.TransactionInfo?> LookupPendingTransaction(Gateway.TransactionLookupIdentifier lookup, CancellationToken token = default)
    {
        var hash = lookup.ValueHex.ConvertFromHex();
        var query = _rwDbContext.PendingTransactions.AsQueryable();

        switch (lookup.Origin)
        {
            case Gateway.TransactionLookupOrigin.Intent:
                query = query.Where(mt => mt.IntentHash == hash);
                break;
            case Gateway.TransactionLookupOrigin.SignedIntent:
                throw new NotImplementedException("fix me"); // TODO fix me
            case Gateway.TransactionLookupOrigin.Notarized:
                throw new NotImplementedException("fix me"); // TODO fix me
            case Gateway.TransactionLookupOrigin.Payload:
                query = query.Where(lt => lt.PayloadHash == hash);
                break;
            default:
                throw new ArgumentOutOfRangeException("fix me"); // TODO fix me
        }

        // We lookup the mempool transaction using the _rwDbContext which is bound to the
        // ReadWriteDbContext so that it gets the most recent details -- to ensure that submitted transactions
        // are immediately shown as pending.
        var mempoolTransaction = await query.SingleOrDefaultAsync(token);

        if (mempoolTransaction is null)
        {
            return null;
        }

        var stateVersion = -1; // TODO fix me

        var status = mempoolTransaction.Status switch
        {
            // If it is committed here, but not on ledger - it's likely because the read replica hasn't caught up yet
            PendingTransactionStatus.Committed => new Gateway.TransactionStatus(stateVersion, Gateway.TransactionStatus.StatusEnum.Succeeded), // TODO , transactionContents.ConfirmedTime),
            PendingTransactionStatus.SubmittedOrKnownInNodeMempool => new Gateway.TransactionStatus(stateVersion, Gateway.TransactionStatus.StatusEnum.Pending),
            PendingTransactionStatus.Missing => new Gateway.TransactionStatus(stateVersion, Gateway.TransactionStatus.StatusEnum.Pending),
            PendingTransactionStatus.ResolvedButUnknownTillSyncedUp => new Gateway.TransactionStatus(stateVersion, Gateway.TransactionStatus.StatusEnum.Pending),
            PendingTransactionStatus.Failed => new Gateway.TransactionStatus(stateVersion, Gateway.TransactionStatus.StatusEnum.Failed),
            _ => throw new ArgumentOutOfRangeException(),
        };

        return new Gateway.TransactionInfo(
            transactionStatus: status,
            payloadHashHex: Array.Empty<byte>().ToHex(),
            intentHashHex: Array.Empty<byte>().ToHex(),
            transactionAccumulatorHex: Array.Empty<byte>().ToHex(),
            feePaid: new Gateway.TokenAmount("0", new Gateway.TokenIdentifier("some rri")) // TODO TokenAmount.FromSubUnitsString(transactionContents.FeePaidSubunits).AsGatewayTokenAmount(_networkConfigurationProvider.GetXrdTokenIdentifier())
        );
    }

    private async Task<List<long>> GetRecentUserTransactionStateVersions(
        RecentTransactionPageRequest request,
        Gateway.LedgerState atLedgerState,
        Gateway.LedgerState? fromLedgerState,
        CancellationToken token)
    {
        if (fromLedgerState != null)
        {
            var bottomStateVersionBoundary = request.Cursor?.StateVersionBoundary ?? fromLedgerState._Version;
            var topStateVersionBoundary = atLedgerState._Version;

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
            var topStateVersionBoundary = request.Cursor?.StateVersionBoundary ?? atLedgerState._Version;

            return await _dbContext.LedgerTransactions
                .Where(lt =>
                    lt.StateVersion <= topStateVersionBoundary
                    && lt.IsUserTransaction
                )
                .OrderByDescending(at => at.StateVersion)
                .Take(request.PageSize + 1)
                .Select(at => at.StateVersion)
                .ToListAsync(token);
        }
    }

    private async Task<List<Gateway.TransactionInfo>> GetTransactions(List<long> transactionStateVersions, CancellationToken token)
    {
        var transactions = await _dbContext.LedgerTransactions
            .Where(lt => transactionStateVersions.Contains(lt.StateVersion))
            .OrderByDescending(lt => lt.StateVersion)
            .ToListAsync(token);

        return transactions.Select(MapToGatewayAccountTransaction).ToList();
    }

    private async Task<LookupResult> GetTransactionWithDetails(long stateVersion, CancellationToken token)
    {
        var transaction = await _dbContext.LedgerTransactions
            .Where(lt => lt.StateVersion == stateVersion)
            .Include(lt => lt.RawTransaction)
            .OrderByDescending(lt => lt.StateVersion)
            .FirstAsync(token);

        List<Entity> referencedEntities = new List<Entity>();

        if (transaction.ReferencedEntities.Any())
        {
            referencedEntities = await _dbContext.Entities
                .Where(e => transaction.ReferencedEntities.Contains(e.Id))
                .ToListAsync(token);
        }

        return MapToGatewayAccountTransactionWithDetails(transaction, referencedEntities);
    }

    private Gateway.TransactionInfo MapToGatewayAccountTransaction(LedgerTransaction ledgerTransaction)
    {
        return new Gateway.TransactionInfo(
            transactionStatus: new Gateway.TransactionStatus(ledgerTransaction.StateVersion, ToGatewayStatus(ledgerTransaction.Status), ledgerTransaction.RoundTimestamp),
            payloadHashHex: ledgerTransaction.PayloadHash.ToHex(),
            intentHashHex: ledgerTransaction.IntentHash.ToHex(),
            transactionAccumulatorHex: ledgerTransaction.TransactionAccumulator.ToHex(),
            feePaid: ledgerTransaction.FeePaid.AsGatewayTokenAmount(_networkConfigurationProvider.GetXrdTokenIdentifier())
        );
    }

    private LookupResult MapToGatewayAccountTransactionWithDetails(LedgerTransaction ledgerTransaction, List<Entity> referencedEntities)
    {
        return new LookupResult(MapToGatewayAccountTransaction(ledgerTransaction), new Gateway.TransactionDetails(
            rawHex: ledgerTransaction.RawTransaction!.Payload.ToHex(),
            referencedGlobalEntities: referencedEntities.Where(re => re.GlobalAddress != null).Select(re => re.BuildHrpGlobalAddress(_networkConfigurationProvider.GetHrpDefinition())).ToList(),
            messageHex: ledgerTransaction.Message?.ToHex()
        ));
    }

    private Gateway.TransactionStatus.StatusEnum ToGatewayStatus(LedgerTransactionStatus status)
    {
        return status switch
        {
            LedgerTransactionStatus.Succeeded => Gateway.TransactionStatus.StatusEnum.Succeeded,
            LedgerTransactionStatus.Failed => Gateway.TransactionStatus.StatusEnum.Failed,
            LedgerTransactionStatus.Rejected => Gateway.TransactionStatus.StatusEnum.Rejected,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null),
        };
    }
}
