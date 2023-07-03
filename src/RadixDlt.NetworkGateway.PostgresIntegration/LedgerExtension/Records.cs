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

using RadixDlt.NetworkGateway.Abstractions;
using RadixDlt.NetworkGateway.Abstractions.Model;
using RadixDlt.NetworkGateway.Abstractions.Numerics;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension;

internal record FungibleVaultChange(ReferencedEntity ReferencedVault, ReferencedEntity ReferencedResource, TokenAmount Balance, long StateVersion);

internal record NonFungibleVaultChange(ReferencedEntity ReferencedVault, ReferencedEntity ReferencedResource, string NonFungibleId, bool IsWithdrawal, long StateVersion);

internal record NonFungibleIdChange(ReferencedEntity ReferencedResource, string NonFungibleId, bool IsDeleted, bool IsLocked, byte[]? MutableData, long StateVersion);

internal record MetadataChange(ReferencedEntity ReferencedEntity, string Key, byte[]? Value, bool IsDeleted, bool IsLocked, long StateVersion); // TODO use ScryptoSbor.String/ValueButes Key, ScryptoSbor.Enum/ValueBytes? Value

internal record ResourceSupplyChange(long ResourceEntityId, long StateVersion, TokenAmount? TotalSupply = null, TokenAmount? Minted = null, TokenAmount? Burned = null);

internal record ValidatorSetChange(long Epoch, IDictionary<ValidatorKeyLookup, TokenAmount> ValidatorSet, long StateVersion);

internal record PackageChange(long PackageEntityId, byte[] CodeHash, byte[] Code, PackageVmType VmType, byte[] SchemaHash, byte[] Schema, string BlueprintName, string BlueprintVersion, string Blueprint, long StateVersion);

internal record PackageChangeBuilder(long PackageEntityId, long StateVersion)
{
    private byte[]? _codeHash;
    private byte[]? _code;
    private PackageVmType? _vmType;
    private byte[]? _schemaHash;
    private byte[]? _schema;
    private string? _blueprintName;
    private string? _blueprintVersion;
    private string? _blueprint;

    public void WithVmType(PackageVmType vmType)
    {
        _vmType = vmType;
    }

    public void WithCode(byte[] codeHash, byte[] code)
    {
        _codeHash = codeHash;
        _code = code;
    }

    public void WithSchema(byte[] schemaHash, byte[] schema)
    {
        _schemaHash = schemaHash;
        _schema = schema;
    }

    public void WithBlueprint(string name, string version, string blueprintJson)
    {
        _blueprintName = name;
        _blueprintVersion = version;
        _blueprint = blueprintJson;
    }

    public PackageChange Build()
    {
        return new PackageChange(
            PackageEntityId,
            _codeHash ?? throw CreateMissingPropertyException(nameof(_codeHash)),
            _code ?? throw CreateMissingPropertyException(nameof(_code)),
            _vmType ?? throw CreateMissingPropertyException(nameof(_vmType)),
            _schemaHash ?? throw CreateMissingPropertyException(nameof(_schemaHash)),
            _schema ?? throw CreateMissingPropertyException(nameof(_schema)),
            _blueprintName ?? throw CreateMissingPropertyException(nameof(_blueprintName)),
            _blueprintVersion ?? throw CreateMissingPropertyException(nameof(_blueprintVersion)),
            _blueprint ?? throw CreateMissingPropertyException(nameof(_blueprint)),
            StateVersion);
    }

    private InvalidOperationException CreateMissingPropertyException(string propertyName)
    {
        return new InvalidOperationException($"Incomplete PackageChange definition of PackageEntityId={PackageEntityId} at StateVersion={StateVersion}, missing {propertyName}.");
    }
}

internal record struct MetadataLookup(long EntityId, string Key);

internal record struct EntityResourceLookup(long EntityId, long ResourceEntityId);

internal record struct EntityResourceVaultLookup(long EntityId, long ResourceEntityId);

internal record struct NonFungibleStoreLookup(long NonFungibleEntityId, long StateVersion);

internal record struct NonFungibleIdLookup(long ResourceEntityId, string NonFungibleId);

internal record struct ValidatorKeyLookup(long ValidatorEntityId, PublicKeyType PublicKeyType, ValueBytes PublicKey);

internal record struct PackageChangeLookup(long PackageEntityId, long StateVersion);
