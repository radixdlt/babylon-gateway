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
using RadixDlt.NetworkGateway.Abstractions.Addressing;
using RadixDlt.NetworkGateway.Abstractions.Extensions;
using RadixDlt.NetworkGateway.GatewayApi.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using CoreModel = RadixDlt.CoreApiSdk.Model;
using GatewayModel = RadixDlt.NetworkGateway.GatewayApiSdk.Model;
using ToolkitModel = RadixEngineToolkit;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Services;

internal interface IVirtualEntityDataProvider
{
    public bool IsVirtualAccountAddress(EntityAddress address);

    public bool IsVirtualIdentityAddress(EntityAddress address);

    (GatewayModel.StateEntityDetailsResponseComponentDetails Details, GatewayModel.EntityMetadataCollection Metadata) GetVirtualEntityData(EntityAddress address);
}

internal class VirtualEntityDataProvider : IVirtualEntityDataProvider
{
    private readonly string _accountPackage;
    private readonly string _identityPackage;
    private readonly byte _networkId;
    private readonly byte _secp256k1VirtualAccountDiscriminator;
    private readonly byte _ed25519VirtualAccountDiscriminator;
    private readonly byte _secp256k1VirtualIdentityDiscriminator;
    private readonly byte _ed25519VirtualIdentityDiscriminator;
    private readonly string _secp256k1SignatureVirtualBadge;
    private readonly string _ed25519SignatureVirtualBadge;
    private readonly List<GatewayModel.ComponentEntityRoleAssignmentEntry> _virtualEntityRoleAssignmentEntries;

    public VirtualEntityDataProvider(IRoleAssignmentsKeyProvider roleAssignmentsKeyProvider, INetworkConfigurationProvider networkConfigurationProvider)
    {
        _accountPackage = networkConfigurationProvider.GetWellKnownAddresses().AccountPackage;
        _identityPackage = networkConfigurationProvider.GetWellKnownAddresses().IdentityPackage;
        _networkId = networkConfigurationProvider.GetNetworkId();
        _secp256k1VirtualAccountDiscriminator = (byte)networkConfigurationProvider.GetAddressTypeDefinition(AddressEntityType.GlobalVirtualSecp256k1Account).AddressBytePrefix;
        _ed25519VirtualAccountDiscriminator = (byte)networkConfigurationProvider.GetAddressTypeDefinition(AddressEntityType.GlobalVirtualEd25519Account).AddressBytePrefix;
        _secp256k1VirtualIdentityDiscriminator = (byte)networkConfigurationProvider.GetAddressTypeDefinition(AddressEntityType.GlobalVirtualSecp256k1Identity).AddressBytePrefix;
        _ed25519VirtualIdentityDiscriminator = (byte)networkConfigurationProvider.GetAddressTypeDefinition(AddressEntityType.GlobalVirtualEd25519Identity).AddressBytePrefix;
        _secp256k1SignatureVirtualBadge = networkConfigurationProvider.GetWellKnownAddresses().Secp256k1SignatureVirtualBadge;
        _ed25519SignatureVirtualBadge = networkConfigurationProvider.GetWellKnownAddresses().Ed25519SignatureVirtualBadge;
        _virtualEntityRoleAssignmentEntries = GenerateVirtualEntityRoleAssignmentEntries(roleAssignmentsKeyProvider);
    }

    public bool IsVirtualAccountAddress(EntityAddress address)
    {
        return IsAccount(DecodeAddress(address, false));
    }

    public bool IsVirtualIdentityAddress(EntityAddress address)
    {
        return IsIdentity(DecodeAddress(address, false));
    }

    public (GatewayModel.StateEntityDetailsResponseComponentDetails Details, GatewayModel.EntityMetadataCollection Metadata) GetVirtualEntityData(EntityAddress address)
    {
        var decoded = DecodeAddress(address, true);

        if (!IsSecp256k1(decoded) && !IsEd25519(decoded))
        {
            throw new ArgumentException("Failed to detect key algorithm (ed25519 or secp256k1)", nameof(address));
        }

        if (!IsAccount(decoded) && !IsIdentity(decoded))
        {
            throw new ArgumentException("Failed to detect entity type (account or identity)", nameof(address));
        }

        ToolkitModel.PublicKeyHash publicKeyHash = IsSecp256k1(decoded)
            ? new ToolkitModel.PublicKeyHash.Secp256k1(decoded.AddressBytes)
            : new ToolkitModel.PublicKeyHash.Ed25519(decoded.AddressBytes);

        using var ownedKeysItem = new ToolkitModel.MetadataValue.PublicKeyHashArrayValue(new[] { publicKeyHash });
        using var ownerBadgeItem = new ToolkitModel.MetadataValue.NonFungibleLocalIdValue(new ToolkitModel.NonFungibleLocalId.Bytes(decoded.Data));

        var ownerKeysBytes = ToolkitModel.RadixEngineToolkitUniffiMethods.MetadataSborEncode(ownedKeysItem);
        var ownerKeysRawHex = ownerKeysBytes.ToArray().ToHex();
        var ownerKeysProgrammaticJson = ScryptoSborUtils.DataToProgrammaticJson(ownerKeysBytes, _networkId);

        var ownerBadgeBytes = ToolkitModel.RadixEngineToolkitUniffiMethods.MetadataSborEncode(ownerBadgeItem);
        var ownerBadgeRawHex = ownerBadgeBytes.ToArray().ToHex();
        var ownerBadgeProgrammaticJson = ScryptoSborUtils.DataToProgrammaticJson(ownerBadgeBytes, _networkId);

        var roleAssignmentOwnerProofLocalId = new CoreModel.NonFungibleLocalId(
            simpleRep: ToolkitModel.RadixEngineToolkitUniffiMethods.NonFungibleLocalIdAsStr(new ToolkitModel.NonFungibleLocalId.Bytes(decoded.AddressBytes)),
            idType: CoreModel.NonFungibleIdType.Bytes,
            sborHex: ToolkitModel.RadixEngineToolkitUniffiMethods.NonFungibleLocalIdSborEncode(new ToolkitModel.NonFungibleLocalId.Bytes(decoded.AddressBytes)).ToArray().ToHex());
        var roleAssignmentOwnerProofGlobalId = IsSecp256k1(decoded)
            ? new CoreModel.NonFungibleGlobalId(_secp256k1SignatureVirtualBadge, roleAssignmentOwnerProofLocalId)
            : new CoreModel.NonFungibleGlobalId(_ed25519SignatureVirtualBadge, roleAssignmentOwnerProofLocalId);
        var ownerRule = new CoreModel.ProtectedAccessRule(new CoreModel.ProofAccessRuleNode(new CoreModel.RequireProofRule(new CoreModel.NonFungibleRequirement(roleAssignmentOwnerProofGlobalId))));
        var roleAssignmentOwner = new CoreModel.OwnerRole(rule: ownerRule, updater: CoreModel.OwnerRoleUpdater.Object);

        var securifyRule = new[]
        {
            new GatewayModel.ComponentEntityRoleAssignmentEntry(
                new GatewayModel.RoleKey("securify", GatewayModel.ObjectModuleId.Main),
                new GatewayModel.ComponentEntityRoleAssignmentEntryAssignment(GatewayModel.RoleAssignmentResolution.Explicit, ownerRule),
                new List<GatewayModel.RoleKey> { new("_self_", GatewayModel.ObjectModuleId.Main) }),
        };

        var effectiveRoleAssignmentEntries = securifyRule.Concat(_virtualEntityRoleAssignmentEntries).ToList();

        var details = IsAccount(decoded)
            ? new GatewayModel.StateEntityDetailsResponseComponentDetails(
                packageAddress: _accountPackage,
                blueprintName: "Account",
                blueprintVersion: "1.0.0",
                state: new CoreModel.AccountFieldStateValue(CoreModel.DefaultDepositRule.Accept),
                roleAssignments: new GatewayModel.ComponentEntityRoleAssignments(roleAssignmentOwner, effectiveRoleAssignmentEntries),
                royaltyVaultBalance: null)
            : new GatewayModel.StateEntityDetailsResponseComponentDetails(
                packageAddress: _identityPackage,
                blueprintName: "Identity",
                blueprintVersion: "1.0.0",
                state: null,
                roleAssignments: new GatewayModel.ComponentEntityRoleAssignments(roleAssignmentOwner, effectiveRoleAssignmentEntries),
                royaltyVaultBalance: null);

        var ownerKeys = new GatewayModel.EntityMetadataItemValue(ownerKeysRawHex, ownerKeysProgrammaticJson, ScryptoSborUtils.ConvertToolkitMetadataToGateway(ownedKeysItem));
        var ownerBadge = new GatewayModel.EntityMetadataItemValue(ownerBadgeRawHex, ownerBadgeProgrammaticJson, ScryptoSborUtils.ConvertToolkitMetadataToGateway(ownerBadgeItem));

        var metadata = new GatewayModel.EntityMetadataCollection(
            totalCount: 2,
            nextCursor: null,
            items: new List<GatewayModel.EntityMetadataItem>
            {
                new("owner_keys", ownerKeys),
                new("owner_badge", ownerBadge, true),
            }
        );

        return (details, metadata);
    }

    private DecodedRadixAddress DecodeAddress(EntityAddress address, bool strict)
    {
        var decodedAddress = RadixAddressCodec.Decode(address);

        if (strict && decodedAddress.Data.Length != 30)
        {
            throw new ArgumentException("Expected address to be 30 bytes in length.", nameof(address));
        }

        return decodedAddress;
    }

    private bool IsAccount(DecodedRadixAddress decoded)
    {
        return decoded.DiscriminatorByte == _secp256k1VirtualAccountDiscriminator || decoded.DiscriminatorByte == _ed25519VirtualAccountDiscriminator;
    }

    private bool IsIdentity(DecodedRadixAddress decoded)
    {
        return decoded.DiscriminatorByte == _secp256k1VirtualIdentityDiscriminator || decoded.DiscriminatorByte == _ed25519VirtualIdentityDiscriminator;
    }

    private bool IsSecp256k1(DecodedRadixAddress decoded)
    {
        return decoded.DiscriminatorByte == _secp256k1VirtualAccountDiscriminator || decoded.DiscriminatorByte == _secp256k1VirtualIdentityDiscriminator;
    }

    private bool IsEd25519(DecodedRadixAddress decoded)
    {
        return decoded.DiscriminatorByte == _ed25519VirtualAccountDiscriminator || decoded.DiscriminatorByte == _ed25519VirtualIdentityDiscriminator;
    }

    private List<GatewayModel.ComponentEntityRoleAssignmentEntry> GenerateVirtualEntityRoleAssignmentEntries(IRoleAssignmentsKeyProvider roleAssignmentsKeyProvider)
    {
        return roleAssignmentsKeyProvider
            .GetNativeModulesKeys()
            .Select(entry => new GatewayModel.ComponentEntityRoleAssignmentEntry(
                new GatewayModel.RoleKey(entry.Key.Name, entry.Key.ModuleId.ToGatewayModel()),
                new GatewayModel.ComponentEntityRoleAssignmentEntryAssignment(GatewayModel.RoleAssignmentResolution.Owner, null),
                entry.Updaters.Select(x => new GatewayModel.RoleKey(x.Name, x.ModuleId.ToGatewayModel())).ToList()
            ))
            .ToList();
    }
}
