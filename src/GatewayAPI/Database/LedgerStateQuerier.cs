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

using Common.Database;
using Common.Database.Models.Ledger;
using Common.Extensions;
using GatewayAPI.ApiSurface;
using GatewayAPI.Exceptions;
using GatewayAPI.Services;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using RadixGatewayApi.Generated.Model;

namespace GatewayAPI.Database;

public interface ILedgerStateQuerier
{
    Task<GatewayResponse> GetGatewayState();

    Task<LedgerState> GetLedgerState(NetworkIdentifier networkIdentifier, PartialLedgerStateIdentifier? atLedgerStateIdentifier);

    Task<LedgerState> GetTopOfLedgerState();

    void AssertMatchingNetwork(NetworkIdentifier networkIdentifier);
}

public class LedgerStateQuerier : ILedgerStateQuerier
{
    private readonly GatewayReadOnlyDbContext _dbContext;
    private readonly IValidations _validations;
    private readonly INetworkConfigurationProvider _networkConfigurationProvider;

    public LedgerStateQuerier(
        GatewayReadOnlyDbContext dbContext,
        IValidations validations,
        INetworkConfigurationProvider networkConfigurationProvider
    )
    {
        _dbContext = dbContext;
        _validations = validations;
        _networkConfigurationProvider = networkConfigurationProvider;
    }

    public async Task<GatewayResponse> GetGatewayState()
    {
        return new GatewayResponse(
            _networkConfigurationProvider.GetNetworkName().AsNetworkIdentifier(),
            new GatewayApiVersions(
                _networkConfigurationProvider.GetGatewayApiVersion(),
                _networkConfigurationProvider.GetGatewayApiSchemaVersion()
            ),
            await GetLedgerState(),
            new TargetLedgerState() // TODO:NG-12 - fix this once we know what the max ledger state seen by nodes is
        );
    }

    // So that we don't forget to check the network name, add the assertion in here.
    public async Task<LedgerState> GetLedgerState(NetworkIdentifier networkIdentifier, PartialLedgerStateIdentifier? atLedgerStateIdentifier)
    {
        AssertMatchingNetwork(networkIdentifier);
        return await GetLedgerState(atLedgerStateIdentifier);
    }

    public async Task<LedgerState> GetTopOfLedgerState()
    {
        var ledgerState = await GetLedgerStateFromQuery(_dbContext.GetTopLedgerTransaction());

        if (ledgerState == null)
        {
            throw new InvalidStateException("There are no transactions in the database");
        }

        return ledgerState;
    }

    public void AssertMatchingNetwork(NetworkIdentifier networkIdentifier)
    {
        var ledgerNetworkName = _networkConfigurationProvider.GetNetworkName();

        if (networkIdentifier.Network != ledgerNetworkName)
        {
            throw new NetworkNotSupportedException(ledgerNetworkName);
        }
    }

    private Task<LedgerState> GetLedgerState(PartialLedgerStateIdentifier? at = null)
    {
        return at switch
        {
            null or { _Version: 0, Timestamp: null, Epoch: 0, Round: 0 } => GetTopOfLedgerState(),
            { _Version: > 0, Timestamp: null, Epoch: 0, Round: 0 } => GetLedgerStateBeforeStateVersion(at._Version),
            { _Version: 0, Timestamp: { }, Epoch: 0, Round: 0 } => GetLedgerStateBeforeTimestamp(at.Timestamp),
            { _Version: 0, Timestamp: null, Epoch: > 0, Round: >= 0 } => GetLedgerStateAtEpochAndRound(at.Epoch, at.Round),
            _ => throw InvalidRequestException.FromOtherError(
                "The at_state_identifier was not either (A) missing (B) with only a state_version; (C) with only a Timestamp; (D) with only an Epoch; or (E) with only an Epoch and Round"
            ),
        };
    }

    private async Task<LedgerState> GetLedgerStateBeforeStateVersion(long stateVersion)
    {
        var ledgerState = await GetLedgerStateFromQuery(_dbContext.GetLatestLedgerTransactionBeforeStateVersion(stateVersion));

        if (ledgerState == null)
        {
            throw new InvalidStateException("There are no transactions in the database");
        }

        return ledgerState;
    }

    private async Task<LedgerState> GetLedgerStateBeforeTimestamp(string timestamp)
    {
        var validatedTimestamp = _validations.ExtractValidTimestamp("The ledger state timestamp", timestamp);

        if (validatedTimestamp > SystemClock.Instance.GetCurrentInstant())
        {
            throw InvalidRequestException.FromOtherError("Timestamp is in the future");
        }

        var ledgerState = await GetLedgerStateFromQuery(_dbContext.GetLatestLedgerTransactionBeforeTimestamp(validatedTimestamp));

        if (ledgerState == null)
        {
            throw InvalidRequestException.FromOtherError("Timestamp was before the start of the ledger");
        }

        return ledgerState;
    }

    private async Task<LedgerState> GetLedgerStateAtEpochAndRound(long epoch, long round)
    {
        var ledgerState = await GetLedgerStateFromQuery(_dbContext.GetLatestLedgerTransactionAtEpochRound(epoch, round));

        if (ledgerState == null)
        {
            throw InvalidRequestException.FromOtherError($"Epoch {epoch} is beyond the end of the ledger");
        }

        return ledgerState;
    }

    private async Task<LedgerState?> GetLedgerStateFromQuery(IQueryable<LedgerTransaction> query)
    {
        return await query
            .Select(lt => new LedgerState(
                lt.ResultantStateVersion,
                lt.RoundTimestamp.AsUtcIsoDateWithMillisString(),
                lt.Epoch,
                lt.RoundInEpoch
            ))
            .SingleOrDefaultAsync();
    }
}
