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
using Microsoft.Extensions.Logging;
using RadixDlt.NetworkGateway.Abstractions.Addressing;
using RadixDlt.NetworkGateway.Abstractions.Configuration;
using RadixDlt.NetworkGateway.DataAggregator.Services;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoreModel = RadixDlt.CoreApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Services;

/// <summary>
/// This captures the NetworkConfiguration for the DataAggregator - typically from a node, but it can also pull it
/// from the database.
/// It persists a local copy of it for the duration of the DataAggregator's uptime.
/// </summary>
internal class NetworkConfigurationProvider : INetworkConfigurationProvider
{
    private readonly TaskCompletionSource<NetworkConfiguration> _tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly IDbContextFactory<ReadWriteDbContext> _dbContextFactory;
    private readonly ILogger<NetworkConfigurationProvider> _logger;

    public NetworkConfigurationProvider(IDbContextFactory<ReadWriteDbContext> dbContextFactory, ILogger<NetworkConfigurationProvider> logger)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
    }

    public async Task SetNetworkConfigurationOrAssertMatching(
        CoreModel.NetworkConfigurationResponse inputNetworkConfiguration,
        CoreModel.NetworkStatusResponse inputNetworkStatus,
        CancellationToken token)
    {
        var networkConfiguration = Map(inputNetworkConfiguration, inputNetworkStatus);
        var existingNetworkConfiguration = await GetCurrentLedgerNetworkConfigurationFromDb(token);

        _tcs.TrySetResult(networkConfiguration);

        if (existingNetworkConfiguration != null)
        {
            if (!existingNetworkConfiguration.HasEqualConfiguration(networkConfiguration))
            {
                throw new ConfigurationException("Network configuration does does not match those stored in the database.");
            }
        }

        if (!(await GetCapturedConfig()).HasEqualConfiguration(networkConfiguration))
        {
            throw new ConfigurationException("Network configuration does does not match those stored from other nodes.");
        }
    }

    public async Task<bool> SaveLedgerNetworkConfigurationToDatabaseOnInitIfNotExists(CancellationToken token)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(token);

        if (await dbContext.NetworkConfiguration.AsNoTracking().AnyAsync(token))
        {
            return false;
        }

        dbContext.Add(GetCapturedConfig());
        await dbContext.SaveChangesAsync(token);
        return true;
    }

    public async ValueTask<byte> GetNetworkId()
    {
        return (await GetCapturedConfig()).NetworkId;
    }

    public async ValueTask<string> GetNetworkName()
    {
        return (await GetCapturedConfig()).NetworkName;
    }

    public async ValueTask<long> GetGenesisEpoch()
    {
        return (await GetCapturedConfig()).GenesisEpoch;
    }

    public async ValueTask<long> GetGenesisRound()
    {
        return (await GetCapturedConfig()).GenesisRound;
    }

    public async ValueTask<HrpDefinition> GetHrpDefinition()
    {
        return (await GetCapturedConfig()).HrpDefinition;
    }

    public async ValueTask<WellKnownAddresses> GetWellKnownAddresses()
    {
        return (await GetCapturedConfig()).WellKnownAddresses;
    }

    public async ValueTask<AddressTypeDefinition> GetAddressTypeDefinition(AddressEntityType entityType)
    {
        return (await GetCapturedConfig()).AddressTypeDefinitions.First(atd => atd.EntityType == entityType);
    }

    private static NetworkConfiguration Map(CoreModel.NetworkConfigurationResponse networkConfiguration, CoreModel.NetworkStatusResponse networkStatus)
    {
        var wka = networkConfiguration.WellKnownAddresses;
        var at = networkConfiguration.AddressTypes.Select(MapAddressTypeDefinition).ToArray();

        return new NetworkConfiguration
        {
            NetworkId = (byte)networkConfiguration.NetworkId,
            NetworkName = networkConfiguration.Network,
            HrpDefinition = new HrpDefinition(
                GlobalPackage: GetHrpPrefix(at, AddressEntityType.GlobalPackage),
                GlobalGenericComponent: GetHrpPrefix(at, AddressEntityType.GlobalGenericComponent),
                InternalGenericComponent: GetHrpPrefix(at, AddressEntityType.InternalGenericComponent),
                GlobalAccount: GetHrpPrefix(at, AddressEntityType.GlobalAccount),
                GlobalVirtualEd25519Account: GetHrpPrefix(at, AddressEntityType.GlobalVirtualEd25519Account),
                GlobalVirtualSecp256k1Account: GetHrpPrefix(at, AddressEntityType.GlobalVirtualSecp256k1Account),
                GlobalValidator: GetHrpPrefix(at, AddressEntityType.GlobalValidator),
                GlobalIdentity: GetHrpPrefix(at, AddressEntityType.GlobalIdentity),
                GlobalVirtualEd25519Identity: GetHrpPrefix(at, AddressEntityType.GlobalVirtualEd25519Identity),
                GlobalVirtualSecp256k1Identity: GetHrpPrefix(at, AddressEntityType.GlobalVirtualSecp256k1Identity),
                GlobalConsensusManager: GetHrpPrefix(at, AddressEntityType.GlobalConsensusManager),
                GlobalFungibleResource: GetHrpPrefix(at, AddressEntityType.GlobalFungibleResource),
                GlobalNonFungibleResource: GetHrpPrefix(at, AddressEntityType.GlobalNonFungibleResource),
                InternalFungibleVault: GetHrpPrefix(at, AddressEntityType.InternalFungibleVault),
                InternalNonFungibleVault: GetHrpPrefix(at, AddressEntityType.InternalNonFungibleVault),
                InternalKeyValueStore: GetHrpPrefix(at, AddressEntityType.InternalKeyValueStore),
                GlobalAccessController: GetHrpPrefix(at, AddressEntityType.GlobalAccessController)
            ),
            WellKnownAddresses = new WellKnownAddresses(
                Xrd: wka.Xrd,
                Secp256k1SignatureVirtualBadge: wka.Secp256k1SignatureVirtualBadge,
                Ed25519SignatureVirtualBadge: wka.Ed25519SignatureVirtualBadge,
                PackageOfDirectCallerVirtualBadge: wka.PackageOfDirectCallerVirtualBadge,
                GlobalCallerVirtualBadge: wka.GlobalCallerVirtualBadge,
                SystemTransactionBadge: wka.SystemTransactionBadge,
                PackageOwnerBadge: wka.PackageOwnerBadge,
                ValidatorOwnerBadge: wka.ValidatorOwnerBadge,
                AccountOwnerBadge: wka.AccountOwnerBadge,
                IdentityOwnerBadge: wka.IdentityOwnerBadge,
                PackagePackage: wka.PackagePackage,
                ResourcePackage: wka.ResourcePackage,
                AccountPackage: wka.AccountPackage,
                IdentityPackage: wka.IdentityPackage,
                ConsensusManagerPackage: wka.ConsensusManagerPackage,
                AccessControllerPackage: wka.AccessControllerPackage,
                TransactionProcessorPackage: wka.TransactionProcessorPackage,
                MetadataModulePackage: wka.MetadataModulePackage,
                RoyaltyModulePackage: wka.RoyaltyModulePackage,
                AccessRulesPackage: wka.RoleAssignmentModulePackage,
                GenesisHelperPackage: wka.GenesisHelperPackage,
                FaucetPackage: wka.FaucetPackage,
                ConsensusManager: wka.ConsensusManager,
                GenesisHelper: wka.GenesisHelper,
                Faucet: wka.Faucet,
                PoolPackage: wka.PoolPackage
            ),
            AddressTypeDefinitions = at,
            GenesisEpoch = networkStatus.GenesisEpochRound.Epoch,
        };
    }

    private static string GetHrpPrefix(AddressTypeDefinition[] addressTypeDefinitions, AddressEntityType entityType)
    {
        return addressTypeDefinitions.First(at => at.EntityType == entityType).HrpPrefix;
    }

    private static AddressTypeDefinition MapAddressTypeDefinition(CoreModel.AddressType arg)
    {
        return new AddressTypeDefinition(Enum.Parse<AddressEntityType>(arg.EntityType.ToString(), true), arg.HrpPrefix, arg.AddressBytePrefix, arg.AddressByteLength);
    }

    private ValueTask<NetworkConfiguration> GetCapturedConfig()
    {
        if (_tcs.Task.IsCompletedSuccessfully)
        {
            return ValueTask.FromResult(_tcs.Task.Result);
        }

        return new ValueTask<NetworkConfiguration>(_tcs.Task);
    }

    private async Task<NetworkConfiguration?> GetCurrentLedgerNetworkConfigurationFromDb(CancellationToken token)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(token);

        return await dbContext.NetworkConfiguration.AsNoTracking().SingleOrDefaultAsync(token);
    }
}
