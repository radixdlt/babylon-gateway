using RadixDlt.CoreApiSdk.Model;
using System;
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
            entityType: substate!.EntityType,
            substateType: substate.SubstateType,
            packageAddress: substate.PackageAddress,
            blueprintName: substate.BlueprintName
        );
    }

    public static ComponentStateSubstate? CloneSubstate(this ComponentStateSubstate? substate)
    {
        if (substate == null)
        {
            return null;
        }

        return new ComponentStateSubstate(
            entityType: substate.EntityType,
            substateType: substate.SubstateType,
            dataStruct: new DataStruct(
                structData: new SborData(
                    dataHex: substate.DataStruct.StructData.DataHex,
                    dataJson: substate.DataStruct.StructData.DataJson
                ),
                ownedEntities: new List<EntityId>(substate.OwnedEntities),
                referencedEntities: new List<EntityId>(substate.DataStruct.ReferencedEntities)
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
                ownedEntities: new List<EntityId>(substate.DataStruct.OwnedEntities),
                referencedEntities: new List<EntityId>(substate.DataStruct.ReferencedEntities)
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
                immutableData: new DataStruct(
                    structData: new SborData(
                    substate.NonFungibleData.ImmutableData.StructData.DataHex,
                    substate.NonFungibleData.ImmutableData.StructData.DataJson)
                ),
                mutableData: new DataStruct(
                    structData: new SborData(
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
            metadata: new List<ResourceManagerSubstateAllOfMetadata>(substate.Metadata),
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
            substateData: substateData
        );
    }
}
