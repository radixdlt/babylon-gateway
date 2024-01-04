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

using Nito.AsyncEx;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoreApi = RadixDlt.CoreApiSdk.Api;
using CoreModel = RadixDlt.CoreApiSdk.Model;

namespace RadixDlt.NetworkGateway.Abstractions.Network;

public interface INetworkConfigurationProvider
{
    public Task<NetworkConfiguration> GetNetworkConfiguration(CancellationToken token = default);
}

public sealed class NetworkConfigurationProvider : INetworkConfigurationProvider
{
    private readonly CoreApi.StatusApi _statusApi;
    // private readonly IEnumerable<INetworkConfigurationReaderObserver>? _oservers;
    private readonly AsyncLazy<NetworkConfiguration> _factory;

    public NetworkConfigurationProvider()
    {
        _statusApi = new CoreApi.StatusApi(); // TODO use DI
        _factory = new AsyncLazy<NetworkConfiguration>(ReadNetworkConfiguration, AsyncLazyFlags.RetryOnFailure);
    }

    public Task<NetworkConfiguration> GetNetworkConfiguration(CancellationToken token = default)
    {
        return _factory.Task;
    }

    private async Task<NetworkConfiguration> ReadNetworkConfiguration()
    {
        // TODO check if network matches our configuration (or just read network name from our config)

        try
        {
            var configuration = await _statusApi.StatusNetworkConfigurationPostAsync();
            var status = await _statusApi.StatusNetworkStatusPostAsync(new CoreModel.NetworkStatusRequest(configuration.Network));

            var wellKnownAddresses = configuration.WellKnownAddresses;
            var addressTypeDefinitions = configuration.AddressTypes
                .Select(at => new AddressTypeDefinition(Enum.Parse<AddressEntityType>(at.EntityType.ToString(), true), at.HrpPrefix, at.AddressBytePrefix, at.AddressByteLength))
                .ToArray();

            string GetHrpPrefix(AddressEntityType entityType)
            {
                return addressTypeDefinitions.First(at => at.EntityType == entityType).HrpPrefix;
            }

            var hrp = new HrpDefinition(
                Suffix: configuration.NetworkHrpSuffix,
                GlobalPackage: GetHrpPrefix(AddressEntityType.GlobalPackage),
                GlobalGenericComponent: GetHrpPrefix(AddressEntityType.GlobalGenericComponent),
                InternalGenericComponent: GetHrpPrefix(AddressEntityType.InternalGenericComponent),
                GlobalAccount: GetHrpPrefix(AddressEntityType.GlobalAccount),
                GlobalVirtualEd25519Account: GetHrpPrefix(AddressEntityType.GlobalVirtualEd25519Account),
                GlobalVirtualSecp256k1Account: GetHrpPrefix(AddressEntityType.GlobalVirtualSecp256k1Account),
                GlobalValidator: GetHrpPrefix(AddressEntityType.GlobalValidator),
                GlobalIdentity: GetHrpPrefix(AddressEntityType.GlobalIdentity),
                GlobalVirtualEd25519Identity: GetHrpPrefix(AddressEntityType.GlobalVirtualEd25519Identity),
                GlobalVirtualSecp256k1Identity: GetHrpPrefix(AddressEntityType.GlobalVirtualSecp256k1Identity),
                GlobalConsensusManager: GetHrpPrefix(AddressEntityType.GlobalConsensusManager),
                GlobalFungibleResource: GetHrpPrefix(AddressEntityType.GlobalFungibleResource),
                GlobalNonFungibleResource: GetHrpPrefix(AddressEntityType.GlobalNonFungibleResource),
                InternalFungibleVault: GetHrpPrefix(AddressEntityType.InternalFungibleVault),
                InternalNonFungibleVault: GetHrpPrefix(AddressEntityType.InternalNonFungibleVault),
                InternalKeyValueStore: GetHrpPrefix(AddressEntityType.InternalKeyValueStore),
                GlobalAccessController: GetHrpPrefix(AddressEntityType.GlobalAccessController)
            );

            var wa = new WellKnownAddresses(
                Xrd: wellKnownAddresses.Xrd,
                Secp256k1SignatureVirtualBadge: wellKnownAddresses.Secp256k1SignatureVirtualBadge,
                Ed25519SignatureVirtualBadge: wellKnownAddresses.Ed25519SignatureVirtualBadge,
                PackageOfDirectCallerVirtualBadge: wellKnownAddresses.PackageOfDirectCallerVirtualBadge,
                GlobalCallerVirtualBadge: wellKnownAddresses.GlobalCallerVirtualBadge,
                SystemTransactionBadge: wellKnownAddresses.SystemTransactionBadge,
                PackageOwnerBadge: wellKnownAddresses.PackageOwnerBadge,
                ValidatorOwnerBadge: wellKnownAddresses.ValidatorOwnerBadge,
                AccountOwnerBadge: wellKnownAddresses.AccountOwnerBadge,
                IdentityOwnerBadge: wellKnownAddresses.IdentityOwnerBadge,
                PackagePackage: wellKnownAddresses.PackagePackage,
                ResourcePackage: wellKnownAddresses.ResourcePackage,
                AccountPackage: wellKnownAddresses.AccountPackage,
                IdentityPackage: wellKnownAddresses.IdentityPackage,
                ConsensusManagerPackage: wellKnownAddresses.ConsensusManagerPackage,
                AccessControllerPackage: wellKnownAddresses.AccessControllerPackage,
                TransactionProcessorPackage: wellKnownAddresses.TransactionProcessorPackage,
                MetadataModulePackage: wellKnownAddresses.MetadataModulePackage,
                RoyaltyModulePackage: wellKnownAddresses.RoyaltyModulePackage,
                RoleAssignmentModulePackage: wellKnownAddresses.RoleAssignmentModulePackage,
                GenesisHelperPackage: wellKnownAddresses.GenesisHelperPackage,
                FaucetPackage: wellKnownAddresses.FaucetPackage,
                ConsensusManager: wellKnownAddresses.ConsensusManager,
                GenesisHelper: wellKnownAddresses.GenesisHelper,
                Faucet: wellKnownAddresses.Faucet,
                PoolPackage: wellKnownAddresses.PoolPackage
            );

            return new NetworkConfiguration((byte)configuration.NetworkId, configuration.Network, status.GenesisEpochRound.Epoch, status.GenesisEpochRound.Round, wa, hrp);
        }
        catch (Exception ex)
        {
            // TODO restore
            Console.Write(ex);
            // await _observers.ForEachAsync(x => x.GetNetworkConfigurationFailed(_nodeConfigProvider.CoreApiNode.Name, ex));

            throw;
        }
    }
}
