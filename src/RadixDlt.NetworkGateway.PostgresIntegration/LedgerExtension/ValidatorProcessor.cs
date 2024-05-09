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

using NpgsqlTypes;
using RadixDlt.NetworkGateway.Abstractions;
using RadixDlt.NetworkGateway.Abstractions.Model;
using RadixDlt.NetworkGateway.Abstractions.Numerics;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using CoreModel = RadixDlt.CoreApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension;

internal record struct ValidatorPublicKeyLookup(long ValidatorEntityId, PublicKeyType PublicKeyType, ValueBytes PublicKey);

internal record ValidatorActiveSet(long Epoch, IDictionary<ValidatorPublicKeyLookup, TokenAmount> ValidatorStake, long StateVersion);

internal class ValidatorProcessor
{
    private readonly ProcessorContext _context;
    private readonly ReferencedEntityDictionary _referencedEntities;

    private List<ValidatorActiveSet> _changes = new();

    /// <summary>
    /// A collection of validator public keys by the earliest state version they were observed on.
    /// </summary>
    private Dictionary<ValidatorPublicKeyLookup, long> _observedPublicKeys = new();
    private Dictionary<ValidatorPublicKeyLookup, ValidatorPublicKeyHistory> _existingPublicKeys = new();

    private List<ValidatorPublicKeyHistory> _publicKeysToAdd = new();
    private List<ValidatorActiveSetHistory> _activeSetsToAdd = new();

    public ValidatorProcessor(ProcessorContext context, ReferencedEntityDictionary referencedEntities)
    {
        _context = context;
        _referencedEntities = referencedEntities;
    }

    public void VisitUpsert(CoreModel.Substate substateData, ReferencedEntity referencedEntity, long stateVersion, long? passingEpoch)
    {
        if (substateData is CoreModel.ConsensusManagerRegisteredValidatorsByStakeIndexEntrySubstate entry)
        {
            var av = entry.Value.ActiveValidator;
            var lookup = new ValidatorPublicKeyLookup(_referencedEntities.Get((EntityAddress)av.Address).DatabaseId, av.Key.KeyType.ToModel(), av.Key.GetKeyBytes());

            _observedPublicKeys.TryAdd(lookup, stateVersion);
        }

        if (substateData is CoreModel.ValidatorFieldStateSubstate state)
        {
            var lookup = new ValidatorPublicKeyLookup(referencedEntity.DatabaseId, state.Value.PublicKey.KeyType.ToModel(), state.Value.PublicKey.GetKeyBytes());

            _observedPublicKeys.TryAdd(lookup, stateVersion);
        }

        if (substateData is CoreModel.ConsensusManagerFieldCurrentValidatorSetSubstate validatorSet)
        {
            if (!passingEpoch.HasValue)
            {
                throw new InvalidOperationException("ConsensusManagerFieldCurrentValidatorSetSubstate can't be processed unless epoch change gets detected");
            }

            var validatorStake = new Dictionary<ValidatorPublicKeyLookup, TokenAmount>();

            foreach (var v in validatorSet.Value.ValidatorSet)
            {
                var lookup = new ValidatorPublicKeyLookup(_referencedEntities.Get((EntityAddress)v.Address).DatabaseId, v.Key.KeyType.ToModel(), v.Key.GetKeyBytes());

                _observedPublicKeys.TryAdd(lookup, stateVersion);
                validatorStake[lookup] = TokenAmount.FromDecimalString(v.Stake);
            }

            _changes.Add(new ValidatorActiveSet(passingEpoch.Value, validatorStake, stateVersion));
        }
    }

    public async Task LoadDependencies()
    {
        _existingPublicKeys.AddRange(await ExistingValidatorPublicKeys());
    }

    public void ProcessChanges()
    {
        foreach (var lookup in _observedPublicKeys.Keys.Except(_existingPublicKeys.Keys))
        {
            var publicKey = new ValidatorPublicKeyHistory
            {
                Id = _context.Sequences.ValidatorPublicKeyHistorySequence++,
                FromStateVersion = _observedPublicKeys[lookup],
                ValidatorEntityId = lookup.ValidatorEntityId,
                KeyType = lookup.PublicKeyType,
                Key = lookup.PublicKey,
            };

            _publicKeysToAdd.Add(publicKey);
            _existingPublicKeys[lookup] = publicKey;
        }

        foreach (var change in _changes)
        {
            foreach (var (lookup, stake) in change.ValidatorStake)
            {
                _activeSetsToAdd.Add(new ValidatorActiveSetHistory
                {
                    Id = _context.Sequences.ValidatorActiveSetHistorySequence++,
                    FromStateVersion = change.StateVersion,
                    Epoch = change.Epoch,
                    ValidatorPublicKeyHistoryId = _existingPublicKeys[lookup].Id,
                    Stake = stake,
                });
            }
        }
    }

    public async Task<int> SaveEntities()
    {
        var rowsInserted = 0;

        rowsInserted += await CopyValidatorPublicKeyHistory();
        rowsInserted += await CopyValidatorActiveSetHistory();

        return rowsInserted;
    }

    private async Task<IDictionary<ValidatorPublicKeyLookup, ValidatorPublicKeyHistory>> ExistingValidatorPublicKeys()
    {
        if (!_observedPublicKeys.Keys.ToHashSet().Unzip(x => x.ValidatorEntityId, x => x.PublicKeyType, x => (byte[])x.PublicKey, out var entityIds, out var keyTypes, out var keys))
        {
            return ImmutableDictionary<ValidatorPublicKeyLookup, ValidatorPublicKeyHistory>.Empty;
        }

        return await _context.ReadHelper.LoadDependencies<ValidatorPublicKeyLookup, ValidatorPublicKeyHistory>(
            @$"
WITH variables (validator_entity_id, key_type, key) AS (
    SELECT UNNEST({entityIds}), UNNEST({keyTypes}), UNNEST({keys})
)
SELECT *
FROM validator_public_key_history
WHERE (validator_entity_id, key_type, key) IN (SELECT * FROM variables);",
            e => new ValidatorPublicKeyLookup(e.ValidatorEntityId, e.KeyType, e.Key));
    }

    private Task<int> CopyValidatorPublicKeyHistory() => _context.WriteHelper.Copy(
        _publicKeysToAdd,
        "COPY validator_public_key_history (id, from_state_version, validator_entity_id, key_type, key) FROM STDIN (FORMAT BINARY)",
        async (writer, e, token) =>
        {
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.ValidatorEntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.KeyType, "public_key_type", token);
            await writer.WriteAsync(e.Key, NpgsqlDbType.Bytea, token);
        });

    private Task<int> CopyValidatorActiveSetHistory() => _context.WriteHelper.Copy(
        _activeSetsToAdd,
        "COPY validator_active_set_history (id, from_state_version, epoch, validator_public_key_history_id, stake) FROM STDIN (FORMAT BINARY)",
        async (writer, e, token) =>
        {
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.Epoch, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.ValidatorPublicKeyHistoryId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.Stake.GetSubUnitsSafeForPostgres(), NpgsqlDbType.Numeric, token);
        });
}
