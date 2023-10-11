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

using Microsoft.Extensions.Options;
using RadixDlt.NetworkGateway.Abstractions;
using RadixDlt.NetworkGateway.Abstractions.Model;
using RadixDlt.NetworkGateway.Abstractions.Numerics;
using RadixDlt.NetworkGateway.GatewayApi.Configuration;
using RadixDlt.NetworkGateway.GatewayApi.Exceptions;
using RadixDlt.NetworkGateway.GatewayApi.Services;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using GatewayModel = RadixDlt.NetworkGateway.GatewayApiSdk.Model;

namespace RadixDlt.NetworkGateway.GatewayApi.Handlers;

internal class DefaultTransactionHandler : ITransactionHandler
{
    private readonly ILedgerStateQuerier _ledgerStateQuerier;
    private readonly ITransactionQuerier _transactionQuerier;
    private readonly ITransactionPreviewService _transactionPreviewService;
    private readonly ITransactionOutcomeService _transactionOutcomeService;
    private readonly ISubmissionService _submissionService;
    private readonly IOptionsSnapshot<EndpointOptions> _endpointConfiguration;

    public DefaultTransactionHandler(
        ILedgerStateQuerier ledgerStateQuerier,
        ITransactionQuerier transactionQuerier,
        ITransactionPreviewService transactionPreviewService,
        ITransactionOutcomeService transactionOutcomeService,
        ISubmissionService submissionService,
        IOptionsSnapshot<EndpointOptions> endpointConfiguration)
    {
        _ledgerStateQuerier = ledgerStateQuerier;
        _transactionQuerier = transactionQuerier;
        _transactionPreviewService = transactionPreviewService;
        _transactionOutcomeService = transactionOutcomeService;
        _submissionService = submissionService;
        _endpointConfiguration = endpointConfiguration;
    }

    public async Task<GatewayModel.TransactionConstructionResponse> Construction(CancellationToken token = default)
    {
        var ledgerState = await _ledgerStateQuerier.GetValidLedgerStateForConstructionRequest(null, token);

        return new GatewayModel.TransactionConstructionResponse(ledgerState);
    }

    public async Task<GatewayModel.TransactionStatusResponse> Status(GatewayModel.TransactionStatusRequest request, CancellationToken token = default)
    {
        var ledgerState = await _ledgerStateQuerier.GetValidLedgerStateForReadRequest(null, token);
        return await _transactionQuerier.ResolveTransactionStatusResponse(ledgerState, request.IntentHash, token);
    }

    public async Task<GatewayModel.TransactionCommittedDetailsResponse> CommittedDetails(GatewayModel.TransactionCommittedDetailsRequest request, CancellationToken token = default)
    {
        var ledgerState = await _ledgerStateQuerier.GetValidLedgerStateForReadRequest(request.AtLedgerState, token);
        var withDetails = true;

        var committedTransaction = await _transactionQuerier.LookupCommittedTransaction(
            request.IntentHash,
            request.OptIns ?? GatewayModel.TransactionDetailsOptIns.Default,
            ledgerState,
            withDetails,
            token);

        if (committedTransaction != null)
        {
            return new GatewayModel.TransactionCommittedDetailsResponse(ledgerState, committedTransaction);
        }

        throw new TransactionNotFoundException(request.IntentHash);
    }

    public async Task<GatewayModel.TransactionPreviewResponse> Preview(GatewayModel.TransactionPreviewRequest request, CancellationToken token = default)
    {
        return await _transactionPreviewService.HandlePreviewRequest(request, token);
    }

    public async Task<GatewayModel.TransactionCommittedOutcomeResponse> Outcome(GatewayModel.TransactionCommittedOutcomeRequest request, CancellationToken token = default)
    {
        var atLedgerState = await _ledgerStateQuerier.GetValidLedgerStateForReadRequest(request.AtLedgerState, token);
        var committedTransaction = await _transactionQuerier.LookupCommittedTransaction(
            request.IntentHash,
            GatewayModel.TransactionDetailsOptIns.Default,
            atLedgerState,
            false,
            token);

        if (committedTransaction == null)
        {
            throw new TransactionNotFoundException(request.IntentHash);
        }

        return await _transactionOutcomeService.HandleOutcomeRequest(atLedgerState, committedTransaction.StateVersion, token);
    }

    public async Task<GatewayModel.TransactionSubmitResponse> Submit(GatewayModel.TransactionSubmitRequest request, CancellationToken token = default)
    {
        var atLedgerState = await _ledgerStateQuerier.GetValidLedgerStateForReadRequest(null, token);
        return await _submissionService.HandleSubmitRequest(atLedgerState, request, token);
    }

    public async Task<GatewayModel.StreamTransactionsResponse> StreamTransactions(GatewayModel.StreamTransactionsRequest request, CancellationToken token = default)
    {
        var atLedgerState = await _ledgerStateQuerier.GetValidLedgerStateForReadRequest(request.AtLedgerState, token);
        var fromLedgerState = await _ledgerStateQuerier.GetValidLedgerStateForReadForwardRequest(request.FromLedgerState, token);

        var kindFilter = request.KindFilter switch
        {
            GatewayModel.StreamTransactionsRequest.KindFilterEnum.All => LedgerTransactionKindFilter.AllAnnotated,
            GatewayModel.StreamTransactionsRequest.KindFilterEnum.User => LedgerTransactionKindFilter.UserOnly,
            GatewayModel.StreamTransactionsRequest.KindFilterEnum.EpochChange => LedgerTransactionKindFilter.EpochChangeOnly,
            null => LedgerTransactionKindFilter.UserOnly,
            _ => throw new UnreachableException($"Didn't expect {request.KindFilter} value"),
        };

        var searchCriteria = new TransactionStreamPageRequestSearchCriteria
        {
            Kind = kindFilter,
        };

        request.AffectedGlobalEntitiesFilter?.ForEach(a => searchCriteria.AffectedGlobalEntities.Add((EntityAddress)a));
        request.ManifestAccountsDepositedIntoFilter?.ForEach(a => searchCriteria.ManifestAccountsDepositedInto.Add((EntityAddress)a));
        request.ManifestAccountsWithdrawnFromFilter?.ForEach(a => searchCriteria.ManifestAccountsWithdrawnFrom.Add((EntityAddress)a));
        request.ManifestResourcesFilter?.ForEach(a => searchCriteria.ManifestResources.Add((EntityAddress)a));
        request.EventsFilter?.ForEach(ef =>
        {
            var eventType = ef.Event switch
            {
                GatewayModel.StreamTransactionsRequestEventFilterItem.EventEnum.Deposit => LedgerTransactionEventFilter.EventType.Deposit,
                GatewayModel.StreamTransactionsRequestEventFilterItem.EventEnum.Withdrawal => LedgerTransactionEventFilter.EventType.Withdrawal,
                _ => throw new UnreachableException($"Didn't expect {ef.Event} value"),
            };

            searchCriteria.Events.Add(new LedgerTransactionEventFilter
            {
                Event = eventType,
                EmitterEntityAddress = ef.EmitterAddress != null ? (EntityAddress)ef.EmitterAddress : null,
                ResourceAddress = ef.ResourceAddress != null ? (EntityAddress)ef.ResourceAddress : null,
            });
        });

        var transactionsPageRequest = new TransactionStreamPageRequest(
            FromStateVersion: fromLedgerState?.StateVersion,
            Cursor: GatewayModel.LedgerTransactionsCursor.FromCursorString(request.Cursor),
            PageSize: request.LimitPerPage ?? _endpointConfiguration.Value.DefaultPageSize,
            AscendingOrder: request.Order == GatewayModel.StreamTransactionsRequest.OrderEnum.Asc,
            SearchCriteria: searchCriteria,
            OptIns: request.OptIns ?? GatewayModel.TransactionDetailsOptIns.Default
        );

        var results = await _transactionQuerier.GetTransactionStream(transactionsPageRequest, atLedgerState, token);

        // NB - We don't return a total here as we don't have an index on user transactions
        return new GatewayModel.StreamTransactionsResponse(
            atLedgerState,
            nextCursor: results.NextPageCursor?.ToCursorString(),
            items: results.Transactions
        );
    }
}
