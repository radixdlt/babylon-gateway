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

using Newtonsoft.Json.Linq;
using RadixDlt.NetworkGateway.Abstractions.Extensions;
using RadixDlt.NetworkGateway.GatewayApi.Exceptions;
using RadixDlt.NetworkGateway.GatewayApi.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GatewayModel = RadixDlt.NetworkGateway.GatewayApiSdk.Model;

namespace RadixDlt.NetworkGateway.GatewayApi.Handlers;

internal class DefaultTransactionHandler : ITransactionHandler
{
    private readonly ILedgerStateQuerier _ledgerStateQuerier;
    private readonly ITransactionQuerier _transactionQuerier;
    private readonly IPreviewService _previewService;
    private readonly ISubmissionService _submissionService;

    public DefaultTransactionHandler(
        ILedgerStateQuerier ledgerStateQuerier,
        ITransactionQuerier transactionQuerier,
        IPreviewService previewService,
        ISubmissionService submissionService)
    {
        _ledgerStateQuerier = ledgerStateQuerier;
        _transactionQuerier = transactionQuerier;
        _previewService = previewService;
        _submissionService = submissionService;
    }

    public async Task<GatewayModel.TransactionConstructionResponse> Construction(CancellationToken token = default)
    {
        var ledgerState = await _ledgerStateQuerier.GetValidLedgerStateForReadRequest(null, token);

        return new GatewayModel.TransactionConstructionResponse(ledgerState);
    }

    public async Task<GatewayModel.TransactionRecentResponse> Recent(GatewayModel.TransactionRecentRequest request, CancellationToken token = default)
    {
        var atLedgerState = await _ledgerStateQuerier.GetValidLedgerStateForReadRequest(request.AtLedgerState, token);
        var fromLedgerState = await _ledgerStateQuerier.GetValidLedgerStateForReadForwardRequest(request.FromLedgerState, token);

        var transactionsPageRequest = new RecentTransactionPageRequest(
            Cursor: GatewayModel.LedgerTransactionsCursor.FromCursorString(request.Cursor),
            PageSize: request.Limit ?? 10
        );

        var results = await _transactionQuerier.GetRecentUserTransactions(transactionsPageRequest, atLedgerState, fromLedgerState, token);

        // NB - We don't return a total here as we don't have an index on user transactions
        return new GatewayModel.TransactionRecentResponse(
            atLedgerState,
            nextCursor: results.NextPageCursor?.ToCursorString(),
            items: results.Transactions
        );
    }

    public async Task<GatewayModel.TransactionStatusResponse> Status(GatewayModel.TransactionStatusRequest request, CancellationToken token = default)
    {
        var identifier = new GatewayModel.TransactionCommittedDetailsRequestIdentifier(GatewayModel.TransactionCommittedDetailsRequestIdentifierType.IntentHash, request.IntentHashHex);
        var ledgerState = await _ledgerStateQuerier.GetValidLedgerStateForReadRequest(request.AtLedgerState, token);
        var committedTransaction = await _transactionQuerier.LookupCommittedTransaction(identifier, ledgerState, false, token);
        var pendingTransactions = await _transactionQuerier.LookupPendingTransactionsByIntentHash(request.IntentHashHex.ConvertFromHex(), token);
        var remainingPendingTransactions = pendingTransactions.Where(pt => pt.PayloadHashHex != committedTransaction?.Info.PayloadHashHex).ToList();

        var status = GatewayModel.TransactionStatus.Unknown;
        var errorMessage = (string?)null;
        var knownPayloads = new List<GatewayModel.TransactionStatusResponseKnownPayloadItem>();

        if (committedTransaction != null)
        {
            status = committedTransaction.Info.TransactionStatus;
            errorMessage = committedTransaction.Info.ErrorMessage;

            knownPayloads.Add(new GatewayModel.TransactionStatusResponseKnownPayloadItem(
                payloadHashHex: committedTransaction.Info.PayloadHashHex,
                status: status,
                errorMessage: committedTransaction.Info.ErrorMessage));
        }
        else if (remainingPendingTransactions.Any())
        {
            status = GatewayModel.TransactionStatus.Pending;
        }

        knownPayloads.AddRange(remainingPendingTransactions.Select(pt => new GatewayModel.TransactionStatusResponseKnownPayloadItem(
            payloadHashHex: pt.PayloadHashHex,
            status: pt.Status,
            errorMessage: pt.ErrorMessage)));

        return new GatewayModel.TransactionStatusResponse(ledgerState, status, knownPayloads, errorMessage);
    }

    public async Task<GatewayModel.TransactionCommittedDetailsResponse> CommittedDetails(GatewayModel.TransactionCommittedDetailsRequest request, CancellationToken token = default)
    {
        var ledgerState = await _ledgerStateQuerier.GetValidLedgerStateForReadRequest(request.AtLedgerState, token);
        var committedTransaction = await _transactionQuerier.LookupCommittedTransaction(request.TransactionIdentifier, ledgerState, true, token);

        if (committedTransaction != null)
        {
            return new GatewayModel.TransactionCommittedDetailsResponse(ledgerState, committedTransaction.Info, committedTransaction.Details);
        }

        throw new TransactionNotFoundException(request.TransactionIdentifier);
    }

    public async Task<object> Preview(JToken request, CancellationToken token = default)
    {
        return await _previewService.HandlePreviewRequest(request, token);
    }

    public async Task<GatewayModel.TransactionSubmitResponse> Submit(GatewayModel.TransactionSubmitRequest request, CancellationToken token = default)
    {
        return await _submissionService.HandleSubmitRequest(request, token);
    }
}
