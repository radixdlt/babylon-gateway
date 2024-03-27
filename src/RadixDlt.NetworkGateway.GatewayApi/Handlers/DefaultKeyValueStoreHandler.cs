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

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RadixDlt.NetworkGateway.Abstractions;
using RadixDlt.NetworkGateway.Abstractions.Extensions;
using RadixDlt.NetworkGateway.GatewayApi.Configuration;
using RadixDlt.NetworkGateway.GatewayApi.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GatewayModel = RadixDlt.NetworkGateway.GatewayApiSdk.Model;
using ToolkitModel = RadixEngineToolkit;

namespace RadixDlt.NetworkGateway.GatewayApi.Handlers;

internal class DefaultKeyValueStoreHandler : IKeyValueStoreHandler
{
    private readonly ILedgerStateQuerier _ledgerStateQuerier;
    private readonly IKeyValueStoreQuerier _keyValueStoreQuerier;
    private readonly ILogger _logger;
    private readonly IOptionsSnapshot<EndpointOptions> _endpointConfiguration;

    public DefaultKeyValueStoreHandler(
        ILedgerStateQuerier ledgerStateQuerier,
        IKeyValueStoreQuerier keyValueStoreQuerier,
        ILogger<DefaultKeyValueStoreHandler> logger,
        IOptionsSnapshot<EndpointOptions> endpointConfiguration)
    {
        _ledgerStateQuerier = ledgerStateQuerier;
        _keyValueStoreQuerier = keyValueStoreQuerier;
        _logger = logger;
        _endpointConfiguration = endpointConfiguration;
    }

    public async Task<GatewayModel.StateKeyValueStoreKeysResponse> Keys(GatewayModel.StateKeyValueStoreKeysRequest request, CancellationToken token = default)
    {
        var ledgerState = await _ledgerStateQuerier.GetValidLedgerStateForReadRequest(request.AtLedgerState, token);

        return await _keyValueStoreQuerier.KeyValueStoreKeys(
            (EntityAddress)request.KeyValueStoreAddress,
            ledgerState,
            GatewayModel.StateKeyValueStoreKeysCursor.FromCursorString(request.Cursor),
            request.LimitPerPage ?? _endpointConfiguration.Value.DefaultPageSize,
            token);
    }

    public async Task<GatewayModel.StateKeyValueStoreDataResponse> Data(GatewayModel.StateKeyValueStoreDataRequest request, CancellationToken token = default)
    {
        var ledgerState = await _ledgerStateQuerier.GetValidLedgerStateForReadRequest(request.AtLedgerState, token);
        var keys = ExtractKeys(request.Keys).ToArray();

        return await _keyValueStoreQuerier.KeyValueStoreData((EntityAddress)request.KeyValueStoreAddress, keys, ledgerState, token);
    }

    private IEnumerable<ValueBytes> ExtractKeys(List<GatewayModel.StateKeyValueStoreDataRequestKeyItem> keys)
    {
        foreach (var item in keys)
        {
            byte[]? rawBytes = null;

            if (item.KeyHex != null)
            {
                rawBytes = item.KeyHex.ConvertFromHex();
            }
            else if (item.KeyJson != null)
            {
                try
                {
                    var rawJson = item.KeyJson.ToJson();

                    if (rawJson != null)
                    {
                        var input = new RadixEngineToolkit.ScryptoSborString.ProgrammaticJson(rawJson);
                        rawBytes = ToolkitModel.RadixEngineToolkitUniffiMethods.ScryptoSborEncodeStringRepresentation(input);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Malformed or invalid JSON input.");
                }
            }

            if (rawBytes != null)
            {
                yield return new ValueBytes(rawBytes);
            }
        }
    }
}
