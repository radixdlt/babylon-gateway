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
using RadixDlt.NetworkGateway.Core.Database;
using RadixDlt.NetworkGateway.Core.Database.Models.Ledger;
using RadixDlt.NetworkGateway.Core.Database.Models.Mempool;
using RadixDlt.NetworkGateway.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Gateway = RadixDlt.NetworkGateway.GatewayApiSdk.Model;
using TokenAmount = RadixDlt.NetworkGateway.Core.Numerics.TokenAmount;

namespace RadixDlt.NetworkGateway.GatewayApi.Services;

public interface ITransactionQuerier
{
    Task<TransactionPageWithoutTotal> GetRecentUserTransactions(RecentTransactionPageRequest request, Gateway.LedgerState ledgerState);

    Task<TransactionPageWithTotal> GetAccountTransactions(AccountTransactionPageRequest request, Gateway.LedgerState ledgerState);

    Task<Gateway.TransactionInfo?> LookupCommittedTransaction(
        ValidatedTransactionIdentifier transactionIdentifier,
        Gateway.LedgerState ledgerState
    );

    Task<Gateway.TransactionInfo?> LookupMempoolTransaction(
        ValidatedTransactionIdentifier transactionIdentifier
    );
}

[DataContract]
public record CommittedTransactionPaginationCursor(long? NextPageAtAndBelowStateVersion)
{
    [DataMember(EmitDefaultValue = false, Name = "v")]
    public long? NextPageAtAndBelowStateVersion { get; set; } = NextPageAtAndBelowStateVersion;

    public static CommittedTransactionPaginationCursor? FromCursorString(string? cursorString)
    {
        return Serializations.FromBase64JsonOrDefault<CommittedTransactionPaginationCursor>(cursorString);
    }

    public string ToCursorString()
    {
        return Serializations.AsBase64Json(this);
    }
}

public record TransactionPageWithTotal(
    long TotalRecords,
    CommittedTransactionPaginationCursor? NextPageCursor,
    List<Gateway.TransactionInfo> Transactions
);

public record TransactionPageWithoutTotal(
    CommittedTransactionPaginationCursor? NextPageCursor,
    List<Gateway.TransactionInfo> Transactions
);

public record AccountTransactionPageRequest(
    ValidatedAccountAddress AccountAddress,
    CommittedTransactionPaginationCursor? Cursor,
    int PageSize
);

public record RecentTransactionPageRequest(
    CommittedTransactionPaginationCursor? Cursor,
    int PageSize
);

public class TransactionQuerier : ITransactionQuerier
{
    private readonly ReadOnlyDbContext _dbContext;
    private readonly INetworkConfigurationProvider _networkConfigurationProvider;
    private readonly ISubmissionTrackingService _submissionTrackingService;

    public TransactionQuerier(
        ReadOnlyDbContext dbContext,
        INetworkConfigurationProvider networkConfigurationProvider,
        ISubmissionTrackingService submissionTrackingService
    )
    {
        _dbContext = dbContext;
        _networkConfigurationProvider = networkConfigurationProvider;
        _submissionTrackingService = submissionTrackingService;
    }

    public async Task<TransactionPageWithoutTotal> GetRecentUserTransactions(RecentTransactionPageRequest request, Gateway.LedgerState ledgerState)
    {
        var transactionStateVersionsAndOneMore = await GetRecentUserTransactionStateVersions(request, ledgerState);
        var nextCursor = transactionStateVersionsAndOneMore.Count == request.PageSize + 1
            ? new CommittedTransactionPaginationCursor(transactionStateVersionsAndOneMore.Last())
            : null;

        var transactions = await GetTransactions(
            transactionStateVersionsAndOneMore.Take(request.PageSize).ToList()
        );

        return new TransactionPageWithoutTotal(nextCursor, transactions);
    }

    public async Task<TransactionPageWithTotal> GetAccountTransactions(AccountTransactionPageRequest request, Gateway.LedgerState ledgerState)
    {
        var totalCount = await CountAccountTransactions(request.AccountAddress, ledgerState);
        var transactionStateVersionsAndOneMore = await GetAccountTransactionStateVersions(request, ledgerState);
        var nextCursor = transactionStateVersionsAndOneMore.Count == request.PageSize + 1
            ? new CommittedTransactionPaginationCursor(transactionStateVersionsAndOneMore.Last())
            : null;

        var transactions = await GetTransactions(
            transactionStateVersionsAndOneMore.Take(request.PageSize).ToList()
        );

        return new TransactionPageWithTotal(totalCount, nextCursor, transactions);
    }

    public async Task<Gateway.TransactionInfo?> LookupCommittedTransaction(
        ValidatedTransactionIdentifier transactionIdentifier,
        Gateway.LedgerState ledgerState
    )
    {
        var stateVersion = await _dbContext.LedgerTransactions
            .Where(lt =>
                lt.ResultantStateVersion <= ledgerState._Version
                && (
                    lt.PayloadHash == transactionIdentifier.Bytes
                    || lt.SignedTransactionHash == transactionIdentifier.Bytes
                    || lt.IntentHash == transactionIdentifier.Bytes
                )
            )
            .Select(lt => lt.ResultantStateVersion)
            .SingleOrDefaultAsync();

        return stateVersion == 0
            ? null :
            (await GetTransactions(new List<long> { stateVersion })).First();
    }

    public async Task<Gateway.TransactionInfo?> LookupMempoolTransaction(
        ValidatedTransactionIdentifier transactionIdentifier
    )
    {
        // We lookup the mempool transaction using the _submissionTrackingService which is bound to the
        // ReadWriteDbContext so that it gets the most recent details -- to ensure that submitted transactions
        // are immediately shown as pending.
        var mempoolTransaction = await _submissionTrackingService.GetMempoolTransaction(transactionIdentifier.Bytes);

        if (mempoolTransaction is null)
        {
            return null;
        }

        var transactionContents = mempoolTransaction.GetTransactionContents();

        var status = mempoolTransaction.Status switch
        {
            // If it is committed here, but not on ledger - it's likely because the read replica hasn't caught up yet
            MempoolTransactionStatus.Committed => new Gateway.TransactionStatus(
                Gateway.TransactionStatus.StatusEnum.CONFIRMED,
                transactionContents.ConfirmedTime?.AsUtcIsoDateWithMillisString(),
                transactionContents.LedgerStateVersion ?? 0
            ),
            MempoolTransactionStatus.SubmittedOrKnownInNodeMempool => new Gateway.TransactionStatus(Gateway.TransactionStatus.StatusEnum.PENDING),
            MempoolTransactionStatus.Missing => new Gateway.TransactionStatus(Gateway.TransactionStatus.StatusEnum.PENDING),
            MempoolTransactionStatus.ResolvedButUnknownTillSyncedUp => new Gateway.TransactionStatus(Gateway.TransactionStatus.StatusEnum.PENDING),
            MempoolTransactionStatus.Failed => new Gateway.TransactionStatus(Gateway.TransactionStatus.StatusEnum.FAILED),
            _ => throw new ArgumentOutOfRangeException(),
        };

        return new Gateway.TransactionInfo(
            status,
            new Gateway.TransactionIdentifier(mempoolTransaction.PayloadHash.ToHex()),
            new List<Gateway.Action>(),
            feePaid: TokenAmount.FromSubUnitsString(transactionContents.FeePaidSubunits).AsGatewayTokenAmount(_networkConfigurationProvider.GetXrdTokenIdentifier()),
            new Gateway.TransactionMetadata(
                hex: mempoolTransaction.Payload.ToHex(),
                message: transactionContents.MessageHex
            )
        );
    }

    private async Task<long> CountAccountTransactions(ValidatedAccountAddress accountAddress, Gateway.LedgerState ledgerState)
    {
        return await _dbContext.AccountTransactions
            .Where(at =>
                at.Account.Address == accountAddress.Address
                && at.ResultantStateVersion <= ledgerState._Version
                && !at.LedgerTransaction.IsStartOfEpoch
            )
            .CountAsync();
    }

    private async Task<List<long>> GetRecentUserTransactionStateVersions(RecentTransactionPageRequest request, Gateway.LedgerState ledgerState)
    {
        var stateVersionUpperBound = request.Cursor?.NextPageAtAndBelowStateVersion ?? ledgerState._Version;

        return await _dbContext.LedgerTransactions
            .Where(lt =>
                lt.ResultantStateVersion <= stateVersionUpperBound
                && lt.IsUserTransaction
            )
            .OrderByDescending(at => at.ResultantStateVersion)
            .Take(request.PageSize + 1)
            .Select(at => at.ResultantStateVersion)
            .ToListAsync();
    }

    private async Task<List<long>> GetAccountTransactionStateVersions(AccountTransactionPageRequest request, Gateway.LedgerState ledgerState)
    {
        var stateVersionUpperBound = request.Cursor?.NextPageAtAndBelowStateVersion ?? ledgerState._Version;

        return await _dbContext.AccountTransactions
            .Where(at =>
                at.Account.Address == request.AccountAddress.Address
                && at.ResultantStateVersion <= stateVersionUpperBound
                && at.IsUserTransaction
            )
            .OrderByDescending(at => at.ResultantStateVersion)
            .Take(request.PageSize + 1)
            .Select(at => at.ResultantStateVersion)
            .ToListAsync();
    }

    private async Task<List<Gateway.TransactionInfo>> GetTransactions(List<long> transactionStateVersions)
    {
        var transactions = await _dbContext.LedgerTransactions
            .Where(lt => transactionStateVersions.Contains(lt.ResultantStateVersion))
            .Include(lt => lt.RawTransaction)
            .OrderByDescending(lt => lt.ResultantStateVersion)
            .AsSplitQuery() // See https://docs.microsoft.com/en-us/ef/core/querying/single-split-queries
            .ToListAsync();

        var gatewayTransactions = new List<Gateway.TransactionInfo>();
        foreach (var ledgerTransaction in transactions)
        {
            gatewayTransactions.Add(MapToGatewayAccountTransaction(ledgerTransaction));
        }

        return gatewayTransactions;
    }

    private Gateway.TransactionInfo MapToGatewayAccountTransaction(LedgerTransaction ledgerTransaction)
    {
        return new Gateway.TransactionInfo(
            new Gateway.TransactionStatus(
                Gateway.TransactionStatus.StatusEnum.CONFIRMED,
                confirmedTime: ledgerTransaction.RoundTimestamp.AsUtcIsoDateWithMillisString(),
                ledgerStateVersion: ledgerTransaction.ResultantStateVersion
            ),
            ledgerTransaction.PayloadHash.AsGatewayTransactionIdentifier(),
            new List<Gateway.Action>(), // TODO: Remove
            ledgerTransaction.FeePaid.AsGatewayTokenAmount(_networkConfigurationProvider.GetXrdTokenIdentifier()),
            new Gateway.TransactionMetadata(
                hex: ledgerTransaction.RawTransaction!.Payload.ToHex(),
                message: ledgerTransaction.Message?.ToHex()
            )
        );
    }
}
