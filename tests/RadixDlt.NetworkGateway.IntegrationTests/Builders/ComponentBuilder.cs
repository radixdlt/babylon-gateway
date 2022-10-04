using RadixDlt.CoreApiSdk.Model;
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

public class ComponentBuilder : BuilderBase<StateUpdates>
{
    private string _componentAddress;

    private Substate? _substate;
    private EntityType _entityType = EntityType.Component;
    private SubstateType _substateType = SubstateType.System;

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

    public override StateUpdates Build()
    {
        if (_substate == null && _substate == null)
        {
            throw new NullReferenceException("No sub state found.");
        }

        var downSubstates = new List<DownSubstate>();

        var downVirtualSubstates = new List<SubstateId>();

        var newGlobalEntities = new List<GlobalEntityId>()
        {
            new GlobalEntityId(
                entityType: _entityType,
                entityAddressHex: AddressHelper.AddressToHex(_componentAddress),
                globalAddressHex: AddressHelper.AddressToHex(_componentAddress),
                globalAddress: _componentAddress),
        };

        var upSubstates = new List<UpSubstate>();

        if (_substate != null)
        {
            upSubstates.Add(
                new UpSubstate(
                    substateId: new SubstateId(
                        entityType: _entityType,
                        entityAddressHex: AddressHelper.AddressToHex(_componentAddress),
                        substateType: _substateType,
                        substateKeyHex: "00"
                    ),
                    substateData: _substate,
                    substateHex: "110d000000436f6d706f6e656e74496e666f010000001003000000801b0000000100000000000000000000000000000000000000000000000000010c09000000537973466175636574301000000000",
                    substateDataHash: "bdd0d7acd8e2436ba42f830f6e732ad287576abc6770df397cc356f780bfe9f2",
                    version: 0L
                )
            );
        }

        return new StateUpdates(downVirtualSubstates, upSubstates, downSubstates, newGlobalEntities);
    }

    public ComponentBuilder WithFixedAddress(string componentAddress)
    {
        _componentAddress = componentAddress;

        return this;
    }

    public ComponentBuilder WithComponentInfoSubstate(string packageAddress, string blueprintName = "")
    {
        _entityType = EntityType.Component;
        _substateType = SubstateType.ComponentInfo;

        _substate = new Substate(
            actualInstance: new ComponentInfoSubstate(
                entityType: _entityType,
                substateType: _substateType,
                packageAddress: packageAddress,
                blueprintName: blueprintName
            )
        );

        return this;
    }

    public ComponentBuilder WithComponentInfoSubstate(ComponentInfoSubstate componentInfoSubstate)
    {
        _entityType = EntityType.Component;
        _substateType = SubstateType.ComponentInfo;

        _substate = new Substate(componentInfoSubstate);

        return this;
    }

    public ComponentBuilder WithComponentStateSubstate(ComponentStateSubstate componentStateSubstate)
    {
        _entityType = EntityType.Component;
        _substateType = SubstateType.ComponentState;

        _substate = new Substate(componentStateSubstate);

        return this;
    }

    public ComponentBuilder WithSystemStateSubstate(long epoch)
    {
        _entityType = EntityType.System;
        _substateType = SubstateType.System;

        _substate = new Substate(new SystemSubstate(
            _entityType, _substateType, epoch));

        return this;
    }

    public ComponentBuilder WithVault(string vaultAddressHex)
    {
        return this;
    }
}
