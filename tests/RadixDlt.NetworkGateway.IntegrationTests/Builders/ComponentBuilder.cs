using RadixDlt.CoreApiSdk.Model;
using RadixDlt.NetworkGateway.IntegrationTests.CoreApiStubs;
using RadixDlt.NetworkGateway.IntegrationTests.Data;
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
    private string _componentAddressHex;
    private string _componentAddressHrp;

    private Substate? _componentInfoSubstate;
    private Substate? _componentStateSubstate;
    private Substate? _componentSystemSubstate;

    public ComponentBuilder(ComponentHrp componentHrp = ComponentHrp.NormalComponentHrp)
    {
        switch (componentHrp)
        {
            case ComponentHrp.AccountComponentHrp:
                _componentAddressHrp = GenesisData.NetworkDefinition.AccountComponentHrp;
                break;
            case ComponentHrp.NormalComponentHrp:
                _componentAddressHrp = GenesisData.NetworkDefinition.NormalComponentHrp;
                break;
            case ComponentHrp.SystemComponentHrp:
                _componentAddressHrp = GenesisData.NetworkDefinition.SystemComponentHrp;
                break;
            default:
                throw new NotImplementedException();
        }

        _componentAddress = AddressHelper.GenerateRandomAddress(_componentAddressHrp);
        _componentAddressHex = AddressHelper.AddressToHex(_componentAddress);
    }

    public override StateUpdates Build()
    {
        if (_componentInfoSubstate == null &&
            _componentStateSubstate == null &&
            _componentSystemSubstate == null)
        {
            throw new NullReferenceException("No sub state found.");
        }

        var downSubstates = new List<DownSubstate>();

        var downVirtualSubstates = new List<SubstateId>();

        var newGlobalEntities = new List<GlobalEntityId>();

        if (_componentInfoSubstate != null)
        {
            newGlobalEntities.Add(new(
                entityType: ((ComponentInfoSubstate)_componentInfoSubstate!.ActualInstance).EntityType,
                entityAddressHex: AddressHelper.AddressToHex(_componentAddress),
                globalAddressHex: AddressHelper.AddressToHex(_componentAddress),
                globalAddress: _componentAddress));
        }

        if (_componentStateSubstate != null)
        {
            newGlobalEntities.Add(new(
                entityType: ((ComponentStateSubstate)_componentStateSubstate!.ActualInstance).EntityType,
                entityAddressHex: AddressHelper.AddressToHex(_componentAddress),
                globalAddressHex: AddressHelper.AddressToHex(_componentAddress),
                globalAddress: _componentAddress));
        }

        if (_componentSystemSubstate != null)
        {
            newGlobalEntities.Add(new(
                entityType: ((SystemSubstate)_componentSystemSubstate!.ActualInstance).EntityType,
                entityAddressHex: AddressHelper.AddressToHex(_componentAddress),
                globalAddressHex: AddressHelper.AddressToHex(_componentAddress),
                globalAddress: _componentAddress));
        }

        var upSubstates = new List<UpSubstate>();

        if (_componentInfoSubstate != null)
        {
            upSubstates.Add(
                new UpSubstate(
                    substateId: new SubstateId(
                        entityType: ((ComponentInfoSubstate)_componentInfoSubstate!.ActualInstance).EntityType,
                        entityAddressHex: AddressHelper.AddressToHex(_componentAddress),
                        substateType: ((ComponentInfoSubstate)_componentInfoSubstate!.ActualInstance).SubstateType,
                        substateKeyHex: "00"
                    ),
                    substateData: _componentInfoSubstate,
                    substateHex: "110d000000436f6d706f6e656e74496e666f010000001003000000801b0000000100000000000000000000000000000000000000000000000000010c09000000537973466175636574301000000000",
                    substateDataHash: "bdd0d7acd8e2436ba42f830f6e732ad287576abc6770df397cc356f780bfe9f2",
                    version: 0L
                )
            );
        }

        if (_componentStateSubstate != null)
        {
            upSubstates.Add(
                new UpSubstate(
                    substateId: new SubstateId(
                        entityType: ((ComponentStateSubstate)_componentStateSubstate!.ActualInstance).EntityType,
                        entityAddressHex: AddressHelper.AddressToHex(_componentAddress),
                        substateType: ((ComponentStateSubstate)_componentStateSubstate!.ActualInstance).SubstateType,
                        substateKeyHex: "00"
                    ),
                    substateData: _componentStateSubstate,
                    substateHex: "110d000000436f6d706f6e656e74496e666f010000001003000000801b0000000100000000000000000000000000000000000000000000000000010c09000000537973466175636574301000000000",
                    substateDataHash: "bdd0d7acd8e2436ba42f830f6e732ad287576abc6770df397cc356f780bfe9f2",
                    version: 0L
                )
            );
        }

        if (_componentSystemSubstate != null)
        {
            upSubstates.Add(
                new UpSubstate(
                    substateId: new SubstateId(
                        entityType: ((SystemSubstate)_componentSystemSubstate!.ActualInstance).EntityType,
                        entityAddressHex: AddressHelper.AddressToHex(_componentAddress),
                        substateType: ((SystemSubstate)_componentSystemSubstate!.ActualInstance).SubstateType,
                        substateKeyHex: "00"
                    ),
                    substateData: _componentSystemSubstate,
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
        _componentAddressHex = AddressHelper.AddressToHex(_componentAddress);

        return this;
    }

    public ComponentBuilder WithFixedAddressHex(string componentAddressHex)
    {
        _componentAddressHex = componentAddressHex;

        _componentAddress = AddressHelper.AddressFromHex(
            _componentAddressHex,
            _componentAddressHrp);

        return this;
    }

    public ComponentBuilder WithComponentInfoSubstate(string packageAddress, string blueprintName = "")
    {
        _componentInfoSubstate = new Substate(
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
        _componentInfoSubstate = new Substate(componentInfoSubstate);

        return this;
    }

    public ComponentBuilder WithComponentStateSubstate(ComponentStateSubstate componentStateSubstate)
    {
        _componentStateSubstate = new Substate(componentStateSubstate);

        return this;
    }

    public ComponentBuilder WithSystemStateSubstate(long epoch)
    {
        _componentSystemSubstate = new Substate(new SystemSubstate(
            EntityType.System, SubstateType.System, epoch));

        return this;
    }
}
