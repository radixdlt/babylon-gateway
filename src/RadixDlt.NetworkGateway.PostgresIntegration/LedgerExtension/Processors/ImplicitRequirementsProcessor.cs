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
using Npgsql;
using NpgsqlTypes;
using RadixDlt.NetworkGateway.Abstractions;
using RadixDlt.NetworkGateway.Abstractions.Extensions;
using RadixDlt.NetworkGateway.Abstractions.StandardMetadata;
using RadixDlt.NetworkGateway.DataAggregator.Services;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using RadixEngineToolkit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using CoreModel = RadixDlt.CoreApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension;

internal class ImplicitRequirementsProcessor : IProcessorBase, ITransactionProcessor, ISubstateUpsertProcessor
{
    private readonly ProcessorContext _context;
    private readonly List<ImplicitRequirement> _newEntriesToAdd = new();

    private record struct GlobalCallerBlueprintKey(string Hash, string BlueprintName, long PackageEntityId);

    private record struct GlobalCallerEntityKey(string Hash, long EntityId);

    private record struct PackageOfDirectCallerKey(string Hash, long PackageEntityId);

    private readonly Dictionary<string, Ed25519PublicKeyImplicitRequirement> _ed25519PublicKeyCopyImplicitRequirements = new();
    private readonly Dictionary<string, Secp256K1PublicKeyImplicitRequirement> _secp256K1PublicKeyImplicitRequirements = new();
    private readonly Dictionary<GlobalCallerBlueprintKey, GlobalCallerBlueprintImplicitRequirement> _globalCallerBlueprintImplicitRequirements = new();
    private readonly Dictionary<GlobalCallerEntityKey, GlobalCallerEntityImplicitRequirement> _globalCallerEntityImplicitRequirements = new();
    private readonly Dictionary<PackageOfDirectCallerKey, PackageOfDirectCallerImplicitRequirement> _packageOfDirectCallerImplicitRequirements = new();

    private readonly ReferencedEntityDictionary _referencedEntityDictionary;
    private readonly CommonDbContext _dbContext;
    private readonly IEnumerable<ILedgerExtenderServiceObserver> _observers;

    public ImplicitRequirementsProcessor(
        ProcessorContext context,
        ReferencedEntityDictionary referencedEntityDictionary,
        CommonDbContext dbContext,
        IEnumerable<ILedgerExtenderServiceObserver> observers)
    {
        _context = context;
        _referencedEntityDictionary = referencedEntityDictionary;
        _dbContext = dbContext;
        _observers = observers;
    }

    public void VisitTransaction(CoreModel.CommittedTransaction transaction, long stateVersion)
    {
        if (transaction.LedgerTransaction is CoreModel.UserLedgerTransaction userLedgerTransactionV1)
        {
            var signerPublicKeys = NotarizedTransactionV1.FromPayloadBytes(userLedgerTransactionV1.NotarizedTransaction.GetPayloadBytes()).SignerPublicKeys();
            foreach (var key in signerPublicKeys)
            {
                ObservePublicKeyHash(key, stateVersion);
            }
        }
        else if (transaction.LedgerTransaction is CoreModel.UserLedgerTransactionV2 userLedgerTransactionV2)
        {
            var signerPublicKeys = NotarizedTransactionV2.FromPayloadBytes(userLedgerTransactionV2.NotarizedTransaction.GetPayloadBytes()).SignerPublicKeys();
            foreach (var key in signerPublicKeys)
            {
                ObservePublicKeyHash(key, stateVersion);
            }
        }

        foreach (var newGlobalEntity in transaction.Receipt.StateUpdates.NewGlobalEntities)
        {
            ObserveGlobalCallerEntityHash((EntityAddress)newGlobalEntity.EntityAddress, stateVersion);

            if (newGlobalEntity.EntityType == CoreModel.EntityType.GlobalPackage)
            {
                ObservePackageOfDirectCallerHash((EntityAddress)newGlobalEntity.EntityAddress, stateVersion);
            }
        }
    }

    public void VisitUpsert(CoreModel.IUpsertedSubstate substate, ReferencedEntity referencedEntity, long stateVersion)
    {
        var substateData = substate.Value.SubstateData;

        if (substateData is CoreModel.PackageBlueprintDefinitionEntrySubstate packageBlueprintDefinition)
        {
            ObserveGlobalCallerBlueprintImplicitRequirement(packageBlueprintDefinition, referencedEntity, stateVersion);
        }
    }

    public async Task LoadDependenciesAsync()
    {
        await Task.CompletedTask;
    }

    public void ProcessChanges()
    {
        _newEntriesToAdd.AddRange(_globalCallerBlueprintImplicitRequirements.Values);
        _newEntriesToAdd.AddRange(_globalCallerEntityImplicitRequirements.Values);
        _newEntriesToAdd.AddRange(_packageOfDirectCallerImplicitRequirements.Values);
        _newEntriesToAdd.AddRange(_secp256K1PublicKeyImplicitRequirements.Values);
        _newEntriesToAdd.AddRange(_ed25519PublicKeyCopyImplicitRequirements.Values);
    }

    public async Task<int> SaveEntitiesAsync()
    {
        var rowsInserted = 0;
        rowsInserted += await CopyImplicitRequirements();
        return rowsInserted;
    }

    private void ObserveGlobalCallerEntityHash(EntityAddress entityAddress, long stateVersion)
    {
        using var retAddress = new Address(entityAddress);

        var globalCallerNfid = RadixEngineToolkitUniffiMethods.DeriveGlobalCallerNonFungibleGlobalIdFromGlobalAddress(retAddress, _context.NetworkConfiguration.Id);

        if (globalCallerNfid.LocalId() is not NonFungibleLocalId.Bytes globalCallerBytes)
        {
            throw new UnreachableException($"Derived global caller should always be NonFungibleLocalId.Bytes but found: {globalCallerNfid.LocalId().GetType()}");
        }

        _globalCallerEntityImplicitRequirements.TryAdd(
            new GlobalCallerEntityKey(globalCallerBytes.value.ToHex(), _referencedEntityDictionary.Get(entityAddress).DatabaseId),
            new GlobalCallerEntityImplicitRequirement
            {
                Id = _context.Sequences.ImplicitRequirementsSequence++,
                Hash = globalCallerBytes.value.ToHex(),
                FirstSeenStateVersion = stateVersion,
                EntityId = _referencedEntityDictionary.Get(entityAddress).DatabaseId,
            });
    }

    private void ObservePackageOfDirectCallerHash(EntityAddress entityAddress, long stateVersion)
    {
        using var retAddress = new Address(entityAddress);

        var packageOfDirectCallerNfid = RadixEngineToolkitUniffiMethods.DerivePackageOfDirectCallerNonFungibleGlobalIdFromPackageAddress(retAddress, _context.NetworkConfiguration.Id);

        if (packageOfDirectCallerNfid.LocalId() is not NonFungibleLocalId.Bytes packageOfDirectCallerBytes)
        {
            throw new UnreachableException($"Derived package of direct caller should always be NonFungibleLocalId.Bytes but found: {packageOfDirectCallerNfid.LocalId().GetType()}");
        }

        _packageOfDirectCallerImplicitRequirements.TryAdd(
            new PackageOfDirectCallerKey(packageOfDirectCallerBytes.value.ToHex(), _referencedEntityDictionary.Get(entityAddress).DatabaseId), new PackageOfDirectCallerImplicitRequirement
            {
                Id = _context.Sequences.ImplicitRequirementsSequence++,
                Hash = packageOfDirectCallerBytes.value.ToHex(),
                FirstSeenStateVersion = stateVersion,
                EntityId = _referencedEntityDictionary.Get(entityAddress).DatabaseId,
            });
    }

    private void ObservePublicKeyHash(PublicKey publicKey, long stateVersion)
    {
        var publicKeyHash = RadixEngineToolkitUniffiMethods.PublicKeyHashFromPublicKey(publicKey);

        switch (publicKeyHash)
        {
            case PublicKeyHash.Ed25519 ed25519:
                _secp256K1PublicKeyImplicitRequirements.TryAdd(
                    ed25519.value.ToHex(), new Secp256K1PublicKeyImplicitRequirement
                    {
                        Id = _context.Sequences.ImplicitRequirementsSequence++, Hash = ed25519.value.ToHex(), FirstSeenStateVersion = stateVersion,
                    });
                break;
            case PublicKeyHash.Secp256k1 secp256K1:
                _secp256K1PublicKeyImplicitRequirements.TryAdd(
                    secp256K1.value.ToHex(), new Secp256K1PublicKeyImplicitRequirement
                    {
                        Id = _context.Sequences.ImplicitRequirementsSequence++, Hash = secp256K1.value.ToHex(), FirstSeenStateVersion = stateVersion,
                    });
                break;

            default:
                throw new NotSupportedException($"Not supported public key type: {publicKeyHash.GetType()}");
        }
    }

    private void ObserveGlobalCallerBlueprintImplicitRequirement(
        CoreModel.PackageBlueprintDefinitionEntrySubstate blueprintDefinitionEntrySubstate,
        ReferencedEntity referencedEntity,
        long stateVersion)
    {
        var packageAddress = new Address(referencedEntity.Address);

        var globalCallerBlueprintNfid = RadixEngineToolkitUniffiMethods.DeriveGlobalCallerNonFungibleGlobalIdFromBlueprintId(
            packageAddress, blueprintDefinitionEntrySubstate.Key.BlueprintName, _context.NetworkConfiguration.Id);

        if (globalCallerBlueprintNfid.LocalId() is not NonFungibleLocalId.Bytes bytes)
        {
            throw new UnreachableException($"Derived blueprint direct caller should always be NonFungibleLocalId.Bytes but found: {globalCallerBlueprintNfid.LocalId().GetType()}");
        }

        _globalCallerBlueprintImplicitRequirements.TryAdd(
            new GlobalCallerBlueprintKey(bytes.value.ToHex(), blueprintDefinitionEntrySubstate.Key.BlueprintName, referencedEntity.DatabaseId),
            new GlobalCallerBlueprintImplicitRequirement
            {
                Id = _context.Sequences.ImplicitRequirementsSequence++,
                Hash = bytes.value.ToHex(),
                FirstSeenStateVersion = stateVersion,
                BlueprintName = blueprintDefinitionEntrySubstate.Key.BlueprintName,
                EntityId = referencedEntity.DatabaseId,
            }
        );
    }

    private async Task<int> CopyImplicitRequirements()
    {
        var entities = _newEntriesToAdd;

        if (entities.Count == 0)
        {
            return 0;
        }

        var connection = (NpgsqlConnection)_dbContext.Database.GetDbConnection();

        var sw = Stopwatch.GetTimestamp();

        await using var createTempTableCommand = connection.CreateCommand();
        createTempTableCommand.CommandText = @"
CREATE TEMP TABLE tmp_implicit_requirements
(LIKE implicit_requirements INCLUDING DEFAULTS)
ON COMMIT DROP";

        await createTempTableCommand.ExecuteNonQueryAsync(_context.Token);

        await using var writer =
            await connection.BeginBinaryImportAsync(
                "COPY tmp_implicit_requirements(id, hash, first_seen_state_version, discriminator, entity_id, blueprint_name) FROM STDIN (FORMAT BINARY)",
                _context.Token);

        foreach (var e in entities)
        {
            var discriminator = _context.WriteHelper.GetDiscriminator<ImplicitRequirementType>(e.GetType());

            await writer.StartRowAsync(_context.Token);
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, _context.Token);
            await writer.WriteAsync(e.Hash, NpgsqlDbType.Text, _context.Token);
            await writer.WriteAsync(e.FirstSeenStateVersion, NpgsqlDbType.Bigint, _context.Token);
            await writer.WriteAsync(discriminator, "implicit_requirement_type", _context.Token);

            if (e is PackageOfDirectCallerImplicitRequirement packageOfDirectCallerImplicitRequirement)
            {
                await writer.WriteAsync(packageOfDirectCallerImplicitRequirement.EntityId, NpgsqlDbType.Bigint, _context.Token);
                await writer.WriteNullAsync(_context.Token);
            }

            if (e is GlobalCallerEntityImplicitRequirement globalCallerEntityImplicitRequirement)
            {
                await writer.WriteAsync(globalCallerEntityImplicitRequirement.EntityId, NpgsqlDbType.Bigint, _context.Token);
                await writer.WriteNullAsync(_context.Token);
            }

            if (e is GlobalCallerBlueprintImplicitRequirement globalCallerBlueprintImplicitRequirement)
            {
                await writer.WriteAsync(globalCallerBlueprintImplicitRequirement.EntityId, NpgsqlDbType.Bigint, _context.Token);
                await writer.WriteAsync(globalCallerBlueprintImplicitRequirement.BlueprintName, NpgsqlDbType.Text, _context.Token);
            }

            if (e is Ed25519PublicKeyImplicitRequirement or Secp256K1PublicKeyImplicitRequirement)
            {
                await writer.WriteNullAsync(_context.Token);
                await writer.WriteNullAsync(_context.Token);
            }
        }

        await writer.CompleteAsync(_context.Token);
        await writer.DisposeAsync();

// TODO PP: check performance here.
        await using var mergeCommand = connection.CreateCommand();
        mergeCommand.CommandText = @"
MERGE INTO implicit_requirements oh
USING tmp_implicit_requirements tmp
ON
    oh.hash = tmp.hash AND
    oh.discriminator = tmp.discriminator AND
    (
        (tmp.discriminator  = 'ed25519public_key') OR
        (tmp.discriminator  = 'secp256k1public_key') OR
        (tmp.discriminator  = 'global_caller_blueprint' AND oh.entity_id = tmp.entity_id AND oh.blueprint_name = tmp.blueprint_name) OR
        (tmp.discriminator  = 'package_of_direct_caller' AND oh.entity_id = tmp.entity_id) OR
        (tmp.discriminator  = 'global_caller_entity' AND oh.entity_id = tmp.entity_id)
    )
WHEN NOT MATCHED THEN INSERT VALUES(id, hash, first_seen_state_version, discriminator, entity_id, blueprint_name);";

        await mergeCommand.ExecuteNonQueryAsync(_context.Token);

        await _observers.ForEachAsync(x => x.StageCompleted(nameof(CopyImplicitRequirements), Stopwatch.GetElapsedTime(sw), entities.Count));

        return entities.Count;
    }
}
