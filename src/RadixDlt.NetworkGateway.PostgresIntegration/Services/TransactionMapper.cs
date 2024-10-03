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
using RadixDlt.NetworkGateway.Abstractions;
using RadixDlt.NetworkGateway.Abstractions.Extensions;
using RadixDlt.NetworkGateway.Abstractions.Model;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Services;

// TODO PP: do we want to move it back to query.
internal static class TransactionMapper
{
    internal static GatewayApiSdk.Model.CommittedTransactionInfo ToGatewayModel(
        this LedgerTransactionQueryResult lt,
        GatewayApiSdk.Model.TransactionDetailsOptIns optIns,
        IDictionary<long, EntityAddress> entityIdToAddressMap,
        List<Event>? events,
        GatewayApiSdk.Model.TransactionBalanceChanges? transactionBalanceChanges)
    {
        string? payloadHash = null;
        string? intentHash = null;
        string? rawHex = null;
        JRaw? message = null;
        string? manifestInstructions = null;
        List<GatewayApiSdk.Model.ManifestClass>? manifestClasses = null;

        if (lt.Discriminator == LedgerTransactionType.User)
        {
            payloadHash = lt.PayloadHash;
            intentHash = lt.IntentHash;
            rawHex = optIns.RawHex ? lt.RawPayload!.ToHex() : null;
            message = lt.Message != null ? new JRaw(lt.Message) : null;
            manifestInstructions = optIns.ManifestInstructions ? lt.ManifestInstructions : null;
            manifestClasses = lt.ManifestClasses.Select(mc => mc.ToGatewayModel()).ToList();
        }

        var receipt = new GatewayApiSdk.Model.TransactionReceipt
        {
            ErrorMessage = lt.ReceiptErrorMessage,
            Status = MapTransactionStatus(lt.ReceiptStatus),
            Output = optIns.ReceiptOutput && lt.ReceiptOutput != null ? new JRaw(lt.ReceiptOutput) : null,
            FeeSummary = optIns.ReceiptFeeSummary ? new JRaw(lt.ReceiptFeeSummary) : null,
            FeeDestination = optIns.ReceiptFeeDestination && lt.ReceiptFeeDestination != null ? new JRaw(lt.ReceiptFeeDestination) : null,
            FeeSource = optIns.ReceiptFeeSource && lt.ReceiptFeeSource != null ? new JRaw(lt.ReceiptFeeSource) : null,
            CostingParameters = optIns.ReceiptCostingParameters ? new JRaw(lt.ReceiptCostingParameters) : null,
            NextEpoch = lt.ReceiptNextEpoch != null ? new JRaw(lt.ReceiptNextEpoch) : null,
            StateUpdates = optIns.ReceiptStateChanges && lt.ReceiptStateUpdates != null ? new JRaw(lt.ReceiptStateUpdates) : null,
            Events = optIns.ReceiptEvents ? events?.Select(x => new GatewayApiSdk.Model.EventsItem(x.Name, new JRaw(x.Emitter), x.Data)).ToList() : null,
        };

        return new GatewayApiSdk.Model.CommittedTransactionInfo(
            stateVersion: lt.StateVersion,
            epoch: lt.Epoch,
            round: lt.RoundInEpoch,
            roundTimestamp: lt.RoundTimestamp.AsUtcIsoDateWithMillisString(),
            transactionStatus: MapTransactionStatus(lt.ReceiptStatus),
            affectedGlobalEntities: optIns.AffectedGlobalEntities ? lt.AffectedGlobalEntities.Select(x => entityIdToAddressMap[x].ToString()).ToList() : null,
            payloadHash: payloadHash,
            intentHash: intentHash,
            feePaid: lt.FeePaid.ToString(),
            confirmedAt: lt.RoundTimestamp,
            errorMessage: lt.ReceiptErrorMessage,
            rawHex: rawHex,
            receipt: receipt,
            message: message,
            balanceChanges: optIns.BalanceChanges ? transactionBalanceChanges : null,
            manifestInstructions: manifestInstructions,
            manifestClasses: manifestClasses
        );
    }

    private static GatewayApiSdk.Model.TransactionStatus MapTransactionStatus(this LedgerTransactionStatus status)
    {
        return status switch
        {
            LedgerTransactionStatus.Succeeded => GatewayApiSdk.Model.TransactionStatus.CommittedSuccess,
            LedgerTransactionStatus.Failed => GatewayApiSdk.Model.TransactionStatus.CommittedFailure,
            _ => throw new UnreachableException($"Didn't expect {status} value"),
        };
    }
}
