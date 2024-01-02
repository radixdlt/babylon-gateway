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

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using Microsoft.OpenApi.Writers;
using RadixDlt.NetworkGateway.GatewayApi;
using RadixDlt.NetworkGateway.GatewayApi.Handlers;
using RadixDlt.NetworkGateway.GatewayApi.Services;
using RadixDlt.NetworkGateway.GatewayApiSdk.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GatewayApi;

public static class OpenApiDocumentHandler
{
    public static async Task Handle(
        [FromServices] INetworkConfigurationProvider networkConfigurationProvider,
        [FromServices] ITransactionHandler transactionHandler,
        [FromServices] ILedgerStateQuerier ledgerStateQuerier,
        HttpContext context,
        CancellationToken token = default)
    {
        var placeholderReplacements = await GetPlaceholderReplacements(networkConfigurationProvider, transactionHandler, ledgerStateQuerier, token);

        var assembly = typeof(GatewayApiBuilder).Assembly;
        var stream = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.gateway-api-schema.yaml");
        var readResult = await new OpenApiStreamReader().ReadAsync(stream);
        var document = readResult.OpenApiDocument;

        RemoveRedoclySpecificTags(document.Tags);

        document.Servers.Clear();
        document.Servers.Add(new OpenApiServer
        {
            Url = "/",
        });

        context.Response.StatusCode = (int)HttpStatusCode.OK;
        context.Response.ContentType = "application/json; charset=utf-8";

        await using var textWriter = new StringWriter(CultureInfo.InvariantCulture);
        var jsonWriter = new OpenApiJsonWriter(textWriter);

        document.SerializeAsV3(jsonWriter);

        var response = textWriter.ToString();

        response = OptionalReplace(response, "<entity-address>", placeholderReplacements.ResourceAddress);
        response = OptionalReplace(response, "<component-entity-address>", placeholderReplacements.ComponentAddress);
        response = OptionalReplace(response, "<transaction-intent-hash>", placeholderReplacements.CommittedTransactionIntentHash);
        response = OptionalReplace(response, "<network-id>", placeholderReplacements.NetworkId?.ToString());
        response = OptionalReplace(response, "<network-name>", placeholderReplacements.NetworkName);
        await context.Response.WriteAsync(response, Encoding.UTF8, token);
    }

    private static void RemoveRedoclySpecificTags(IList<OpenApiTag> tags)
    {
        var examplesTag = tags.Single(x => x.Name.Equals("Examples", StringComparison.InvariantCultureIgnoreCase));
        var architectureTag = tags.Single(x => x.Name.Equals("Architecture", StringComparison.InvariantCultureIgnoreCase));
        var conceptsTag = tags.Single(x => x.Name.Equals("Concepts", StringComparison.InvariantCultureIgnoreCase));

        tags.Remove(examplesTag);
        tags.Remove(architectureTag);
        tags.Remove(conceptsTag);

        var redoclyLinkTag = new OpenApiTag
        {
            Name = "Examples + More Docs",
            Description = @"Please see the full API documentation in [ReDocly](https://radix-babylon-gateway-api.redoc.ly/) for details about the API abstractions and worked examples for many use cases.",
        };

        tags.Insert(1, redoclyLinkTag);
    }

    private static string OptionalReplace(string inputString, string pattern, string? replacement)
    {
        return replacement == null ? inputString : inputString.Replace(pattern, replacement);
    }

    private class PlaceholderReplacements
    {
        public string? ResourceAddress { get; set; }

        public string? ComponentAddress { get; set; }

        public string? CommittedTransactionIntentHash { get; set; }

        public byte? NetworkId { get; set; }

        public string? NetworkName { get; set; }

        public long LedgerStateVersion { get; set; }
    }

    private static async Task<PlaceholderReplacements> GetPlaceholderReplacements(
        INetworkConfigurationProvider networkConfigurationProvider,
        ITransactionHandler transactionHandler,
        ILedgerStateQuerier ledgerStateQuerier,
        CancellationToken token)
    {
        var placeholderReplacements = new PlaceholderReplacements();

        try
        {
            var wellKnownAddresses = networkConfigurationProvider.GetWellKnownAddresses();
            placeholderReplacements.ResourceAddress = wellKnownAddresses.Xrd;
            placeholderReplacements.ComponentAddress = wellKnownAddresses.ConsensusManager;
            placeholderReplacements.NetworkId = networkConfigurationProvider.GetNetworkId();
            placeholderReplacements.NetworkName = networkConfigurationProvider.GetNetworkName();
        }
        catch (Exception)
        {
            // EG not synced up exception: Ignored
        }

        try
        {
            var sampleTransaction = (await transactionHandler.StreamTransactions(new StreamTransactionsRequest(limitPerPage: 1), token)).Items.FirstOrDefault();
            placeholderReplacements.CommittedTransactionIntentHash = sampleTransaction?.IntentHash;
        }
        catch (Exception)
        {
            // EG not synced up exception: Ignored
        }

        return placeholderReplacements;
    }
}
