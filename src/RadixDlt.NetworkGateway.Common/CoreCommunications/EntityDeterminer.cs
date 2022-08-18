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
using RadixDlt.CoreApiSdk.Model;
using RadixDlt.NetworkGateway.Common.Addressing;
using System;
using System.Diagnostics.CodeAnalysis;

namespace RadixDlt.NetworkGateway.Common.CoreCommunications;

public record Entity(
    EntityType EntityType,
    string? AccountAddress = null,
    string? ValidatorAddress = null, // This may be provided along with the AccountAddress for stake entities
    string? ResourceAddress = null,
    long? EpochUnlock = null
);

[SuppressMessage("ReSharper", "InconsistentNaming", Justification = "The underscore identifies the split between identity and sub-entity")]
public enum EntityType
{
    System,
    Account,
    Account_PreparedStake,
    Account_PreparedUnstake,
    Account_ExitingStake,
    Validator,
    Validator_System,
    Resource,
}

public interface IEntityDeterminer
{
    Entity? DetermineEntity(EntityIdentifier? entityIdentifier);

    bool IsXrd(string rri);

    string GetXrdAddress();

    byte[] ParseValidatorPublicKey(string validatorAddress);

    byte[] ParseAccountPublicKey(string accountAddress);

    byte[] ParseResourceRadixEngineAddress(string rri);

    string CreateAccountAddress(byte[] publicKey);
}

public interface INetworkAddressConfigProvider
{
    AddressHrps GetAddressHrps();

    string GetXrdAddress();
}

public class EntityDeterminer : IEntityDeterminer
{
    private readonly ILogger<EntityDeterminer> _logger;
    private readonly INetworkAddressConfigProvider _networkAddressProvider;

    public EntityDeterminer(ILogger<EntityDeterminer> logger, INetworkAddressConfigProvider networkAddressProvider)
    {
        _logger = logger;
        _networkAddressProvider = networkAddressProvider;
    }

    public Entity? DetermineEntity(EntityIdentifier? entityIdentifier)
    {
        if (entityIdentifier == null)
        {
            return null;
        }

        var primaryEntityAddress = entityIdentifier.Address;
        if (primaryEntityAddress == "system")
        {
            return new Entity(EntityType.System);
        }

        if (!RadixAddressParser.TryParse(
                _networkAddressProvider.GetAddressHrps(),
                primaryEntityAddress,
                out var primaryEntityRadixAddress,
                out var errorMessage
            ))
        {
            _logger.LogWarning(
                "Entity address [{Address}] didn't parse correctly: {ErrorMessage}",
                primaryEntityAddress,
                errorMessage
            );
            return null;
        }

        var subEntity = entityIdentifier.SubEntity;

        switch (primaryEntityRadixAddress.Type)
        {
            case RadixAddressType.Account when subEntity == null:
                return new Entity(EntityType.Account, AccountAddress: primaryEntityAddress);
            case RadixAddressType.Account when subEntity.Address == "prepared_stakes":
                return new Entity(EntityType.Account_PreparedStake, AccountAddress: primaryEntityAddress, ValidatorAddress: subEntity.Metadata!.ValidatorAddress);
            case RadixAddressType.Account when subEntity.Address == "prepared_unstakes":
                return new Entity(EntityType.Account_PreparedUnstake, AccountAddress: primaryEntityAddress); // The validator address is part of the StakeUnit resource
            case RadixAddressType.Account when subEntity.Address == "exiting_unstakes":
                return new Entity(EntityType.Account_ExitingStake, AccountAddress: primaryEntityAddress, ValidatorAddress: subEntity.Metadata!.ValidatorAddress, EpochUnlock: subEntity.Metadata.EpochUnlock);
            case RadixAddressType.Account:
                _logger.LogWarning("Unknown account sub-entity address: {SubEntityAddress}", subEntity.Address);
                return null;
            case RadixAddressType.Validator when subEntity == null:
                return new Entity(EntityType.Validator, ValidatorAddress: primaryEntityAddress);
            case RadixAddressType.Validator when subEntity.Address == "system":
                return new Entity(EntityType.Validator_System, ValidatorAddress: primaryEntityAddress);
            case RadixAddressType.Validator:
                _logger.LogWarning("Unknown validator sub-entity address: {SubEntityAddress}", subEntity.Address);
                return null;
            case RadixAddressType.Resource when subEntity == null:
                return new Entity(EntityType.Resource, ResourceAddress: primaryEntityAddress);
            case RadixAddressType.Resource:
                _logger.LogWarning("Unknown resource sub-entity address: {SubEntityAddress}", subEntity.Address);
                return null;
            case RadixAddressType.Node: // A Node address here should not be possible
            default:
                _logger.LogWarning("Unhandled radix address type: {RadixAddressType}", primaryEntityRadixAddress.Type);
                return null;
        }
    }

    public byte[] ParseValidatorPublicKey(string validatorAddress)
    {
        if (!RadixAddressParser.TryParseValidatorAddress(
                _networkAddressProvider.GetAddressHrps(),
                validatorAddress,
                out var radixAddress,
                out var errorMessage
            ))
        {
            throw new Exception($"Validator address [{validatorAddress}] didn't parse correctly: {errorMessage}");
        }

        return radixAddress.CompressedPublicKey;
    }

    public byte[] ParseAccountPublicKey(string accountAddress)
    {
        if (!RadixAddressParser.TryParseAccountAddress(
                _networkAddressProvider.GetAddressHrps(),
                accountAddress,
                out var radixAddress,
                out var errorMessage
            ))
        {
            throw new Exception($"Account address [{accountAddress}] didn't parse correctly: {errorMessage}");
        }

        return radixAddress.CompressedPublicKey;
    }

    public byte[] ParseResourceRadixEngineAddress(string rri)
    {
        if (!RadixAddressParser.TryParseResourceAddress(
                _networkAddressProvider.GetAddressHrps(),
                rri,
                out var radixAddress,
                out var errorMessage
            ))
        {
            throw new Exception($"Resource identifier [{rri}] didn't parse correctly: {errorMessage}");
        }

        return radixAddress.RadixEngineAddress;
    }

    public bool IsXrd(string rri)
    {
        return rri == GetXrdAddress();
    }

    public string GetXrdAddress()
    {
        return _networkAddressProvider.GetXrdAddress();
    }

    public string CreateAccountAddress(byte[] publicKey)
    {
        return RadixBech32.GenerateAccountAddress(_networkAddressProvider.GetAddressHrps().AccountHrp, publicKey);
    }
}
