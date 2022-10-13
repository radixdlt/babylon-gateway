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

using RadixDlt.CoreApiSdk.Model;
using System.Collections.Generic;

namespace RadixDlt.NetworkGateway.IntegrationTests.Utilities;

public static class SubstateExtensions
{
    public static ComponentInfoSubstate? CloneSubstate(this ComponentInfoSubstate? substate)
    {
        if (substate == null)
        {
            return null;
        }

        return new ComponentInfoSubstate(
            substate!.EntityType,
            substate.SubstateType,
            substate.PackageAddress,
            substate.BlueprintName
        );
    }

    public static ComponentStateSubstate? CloneSubstate(this ComponentStateSubstate? substate)
    {
        if (substate == null)
        {
            return null;
        }

        return new ComponentStateSubstate(
            substate.EntityType,
            substate.SubstateType,
            new DataStruct(
                new SborData(
                    substate.DataStruct.StructData.DataHex,
                    substate.DataStruct.StructData.DataJson
                ),
                new List<EntityId>(substate.OwnedEntities),
                new List<EntityId>(substate.DataStruct.ReferencedEntities)
            )
        );
    }

    public static KeyValueStoreEntrySubstate? CloneSubstate(this KeyValueStoreEntrySubstate? substate)
    {
        if (substate == null)
        {
            return null;
        }

        return new KeyValueStoreEntrySubstate(
            substate.EntityType,
            substate.SubstateType,
            substate.KeyHex,
            substate.IsDeleted,
            new DataStruct(
                new SborData(
                    substate.DataStruct.StructData.DataHex,
                    substate.DataStruct.StructData.DataJson
                ),
                new List<EntityId>(substate.DataStruct.OwnedEntities),
                new List<EntityId>(substate.DataStruct.ReferencedEntities)
            )
        );
    }

    public static NonFungibleSubstate? CloneSubstate(this NonFungibleSubstate? substate)
    {
        if (substate == null)
        {
            return null;
        }

        return new NonFungibleSubstate(
            substate.EntityType,
            substate.SubstateType,
            substate.NfIdHex,
            substate.IsDeleted,
            new NonFungibleData(
                new DataStruct(
                    new SborData(
                        substate.NonFungibleData.ImmutableData.StructData.DataHex,
                        substate.NonFungibleData.ImmutableData.StructData.DataJson)
                ),
                new DataStruct(
                    new SborData(
                        substate.NonFungibleData.MutableData.StructData.DataHex,
                        substate.NonFungibleData.MutableData.StructData.DataJson)
                )
            )
        );
    }

    public static PackageSubstate? CloneSubstate(this PackageSubstate? substate)
    {
        if (substate == null)
        {
            return null;
        }

        return new PackageSubstate(
            substate.EntityType,
            substate.SubstateType,
            substate.CodeHex);
    }

    public static ResourceManagerSubstate? CloneSubstate(this ResourceManagerSubstate? substate)
    {
        if (substate == null)
        {
            return null;
        }

        return new ResourceManagerSubstate(
            substate.EntityType,
            substate.SubstateType,
            substate.ResourceType,
            substate.FungibleDivisibility,
            new List<ResourceManagerSubstateAllOfMetadata>(substate.Metadata),
            substate.TotalSupplyAttos);
    }

    public static FungibleResourceAmount? CloneSubstate(this FungibleResourceAmount? substate)
    {
        if (substate == null)
        {
            return null;
        }

        return new FungibleResourceAmount(
            substate.ResourceType,
            substate.ResourceAddress,
            substate.AmountAttos);
    }

    public static NonFungibleResourceAmount? CloneSubstate(this NonFungibleResourceAmount? substate)
    {
        if (substate == null)
        {
            return null;
        }

        return new NonFungibleResourceAmount(
            substate.ResourceType,
            substate.ResourceAddress,
            substate.NfIdsHex);
    }

    public static SystemSubstate? CloneSubstate(this SystemSubstate? substate)
    {
        if (substate == null)
        {
            return null;
        }

        return new SystemSubstate(
            substate.EntityType,
            substate.SubstateType,
            substate.Epoch
        );
    }

    public static VaultSubstate? CloneSubstate(this VaultSubstate? substate)
    {
        if (substate == null)
        {
            return null;
        }

        ResourceAmount? resourceAmount = null;

        if (substate.ResourceAmount.ActualInstance.GetType() == typeof(FungibleResourceAmount))
        {
            resourceAmount = new ResourceAmount(CloneSubstate((substate.ResourceAmount.ActualInstance as FungibleResourceAmount)!));
        }
        else if (substate.GetType() == typeof(NonFungibleResourceAmount))
        {
            resourceAmount = new ResourceAmount(CloneSubstate((substate.ResourceAmount.ActualInstance as NonFungibleResourceAmount)!));
        }

        return new VaultSubstate(
            substate.EntityType,
            substate.SubstateType,
            resourceAmount
        );
    }

    public static DownSubstate? CloneSubstate(this DownSubstate? substate)
    {
        if (substate == null)
        {
            return null;
        }

        return new DownSubstate(
            new SubstateId(
                substate.SubstateId.EntityType,
                substate.SubstateId.EntityAddressHex,
                substate.SubstateId.SubstateType,
                substate.SubstateId.SubstateKeyHex),
            substate.SubstateDataHash,
            substate._Version);
    }

    public static UpSubstate CloneSubstate(this UpSubstate substate)
    {
        var actualInstance = substate.SubstateData.ActualInstance;

        Substate? substateData = null;

        if (actualInstance.GetType() == typeof(ComponentInfoSubstate))
        {
            substateData = new Substate(CloneSubstate((actualInstance as ComponentInfoSubstate)!));
        }
        else if (actualInstance.GetType() == typeof(ComponentStateSubstate))
        {
            substateData = new Substate(CloneSubstate((actualInstance as ComponentStateSubstate)!));
        }
        else if (actualInstance.GetType() == typeof(KeyValueStoreEntrySubstate))
        {
            substateData = new Substate(CloneSubstate((actualInstance as KeyValueStoreEntrySubstate)!));
        }
        else if (actualInstance.GetType() == typeof(NonFungibleSubstate))
        {
            substateData = new Substate(CloneSubstate((actualInstance as NonFungibleSubstate)!));
        }
        else if (actualInstance.GetType() == typeof(PackageSubstate))
        {
            substateData = new Substate(CloneSubstate((actualInstance as PackageSubstate)!));
        }
        else if (actualInstance.GetType() == typeof(ResourceManagerSubstate))
        {
            substateData = new Substate(CloneSubstate((actualInstance as ResourceManagerSubstate)!));
        }
        else if (actualInstance.GetType() == typeof(SystemSubstate))
        {
            substateData = new Substate(CloneSubstate((actualInstance as SystemSubstate)!));
        }
        else if (actualInstance.GetType() == typeof(VaultSubstate))
        {
            substateData = new Substate(CloneSubstate((actualInstance as VaultSubstate)!));
        }

        return new UpSubstate(
            new SubstateId(
                substate.SubstateId.EntityType,
                substate.SubstateId.EntityAddressHex,
                substate.SubstateId.SubstateType,
                substate.SubstateId.SubstateKeyHex
            ),
            substate._Version,
            substate.SubstateHex,
            substate.SubstateDataHash,
            substateData
        );
    }
}
