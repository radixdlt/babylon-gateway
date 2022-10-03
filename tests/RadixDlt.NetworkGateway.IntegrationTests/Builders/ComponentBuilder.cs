﻿using RadixDlt.CoreApiSdk.Model;
using RadixDlt.NetworkGateway.IntegrationTests.CoreApiStubs;
using RadixDlt.NetworkGateway.IntegrationTests.Utilities;
using System;
using System.Collections.Generic;

namespace RadixDlt.NetworkGateway.IntegrationTests.Builders;

public enum ComponentHrp
{
    NormalComponentHrp,
    AccountComponentHrp,
    SystemComponentHrp,
}

public class ComponentBuilder : BuilderBase<(TestGlobalEntity TestGlobalEntity, StateUpdates StateUpdates)>
{
    private string _componentAddress;

    private Substate? _componentInfoSubstateData;
    private Substate? _componentStateSubstateData;
    private string _componentName = string.Empty;

    public ComponentBuilder(CoreApiStubDefaultConfiguration defaultConfig, ComponentHrp componentHrp = ComponentHrp.NormalComponentHrp)
    {
        string strComponentHrp;

        switch (componentHrp)
        {
            case ComponentHrp.AccountComponentHrp:
                strComponentHrp = defaultConfig.NetworkDefinition.AccountComponentHrp;
                break;
            case ComponentHrp.NormalComponentHrp:
                strComponentHrp = defaultConfig.NetworkDefinition.NormalComponentHrp;
                break;
            case ComponentHrp.SystemComponentHrp:
                strComponentHrp = defaultConfig.NetworkDefinition.SystemComponentHrp;
                break;
            default:
                throw new NotImplementedException();
        }

        _componentAddress = AddressHelper.GenerateRandomAddress(strComponentHrp);
    }

    public override (TestGlobalEntity TestGlobalEntity, StateUpdates StateUpdates) Build()
    {
        if (_componentInfoSubstateData == null && _componentStateSubstateData == null)
        {
            throw new NullReferenceException("No subState found.");
        }

        var downSubstates = new List<DownSubstate>();

        var downVirtualSubstates = new List<SubstateId>();

        var globalEntityId = new TestGlobalEntity()
        {
            EntityType = EntityType.Component,
            EntityAddressHex = AddressHelper.AddressToHex(_componentAddress),
            GlobalAddressHex = AddressHelper.AddressToHex(_componentAddress),
            GlobalAddress = _componentAddress,
            Name = _componentName,
        };

        var newGlobalEntities = new List<GlobalEntityId>() { globalEntityId };

        var upSubstates = new List<UpSubstate>();

        if (_componentInfoSubstateData != null)
        {
            upSubstates.Add(
                new UpSubstate(
                    substateId: new SubstateId(
                        entityType: EntityType.Component,
                        entityAddressHex: AddressHelper.AddressToHex(_componentAddress),
                        substateType: SubstateType.ComponentInfo,
                        substateKeyHex: "00"
                    ),
                    substateData: _componentInfoSubstateData,
                    substateHex: "110d000000436f6d706f6e656e74496e666f010000001003000000801b0000000100000000000000000000000000000000000000000000000000010c09000000537973466175636574301000000000",
                    substateDataHash: "bdd0d7acd8e2436ba42f830f6e732ad287576abc6770df397cc356f780bfe9f2",
                    version: 0L
                )
            );
        }

        if (_componentStateSubstateData != null)
        {
            upSubstates.Add(
                new UpSubstate(
                    substateId: new SubstateId(
                        entityType: EntityType.Component,
                        entityAddressHex: AddressHelper.AddressToHex(_componentAddress),
                        substateType: SubstateType.ComponentState,
                        substateKeyHex: "01"
                    ),
                    version: 0L,
                    substateData: _componentStateSubstateData,
                    substateHex: "110e000000436f6d706f6e656e7453746174650100000010010000003007570000001002000000b3240000000000000000000000000000000000000000000000000000000000000000000000000000008324000000000000000000000000000000000000000000000000000000000000000000000001000000",
                    substateDataHash: "810ea50c21903f64f85bce050f7feb679e4df85a657fc20c21bb7ce341b6959d"
                )
            );
        }

        return (globalEntityId, new StateUpdates(downVirtualSubstates, upSubstates, downSubstates, newGlobalEntities));
    }

    public ComponentBuilder WithFixedAddress(string componentAddress)
    {
        _componentAddress = componentAddress;

        return this;
    }

    public ComponentBuilder WithComponentName(string componentName)
    {
        _componentName = componentName;

        return this;
    }

    public ComponentBuilder WithComponentInfoSubstate(string packageAddress, string blueprintName = "")
    {
        _componentInfoSubstateData = new Substate(
            actualInstance: new ComponentInfoSubstate(
                entityType: EntityType.Component,
                substateType: SubstateType.ComponentInfo,
                packageAddress: packageAddress,
                blueprintName: blueprintName
            )
        );

        return this;
    }

    public ComponentBuilder WithComponentInfoSubstate(ComponentInfoSubstate componentInfoSubstate)
    {
        _componentInfoSubstateData = new Substate(componentInfoSubstate);

        return this;
    }

    public ComponentBuilder WithComponentStateSubstate(ComponentStateSubstate componentStateSubstate)
    {
        _componentStateSubstateData = new Substate(componentStateSubstate);

        return this;
    }

    public ComponentBuilder WithVault(string vaultAddressHex)
    {
        return this;
    }
}
