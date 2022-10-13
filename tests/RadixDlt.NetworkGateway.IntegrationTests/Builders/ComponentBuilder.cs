using RadixDlt.CoreApiSdk.Model;
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
    private readonly string _componentAddressHrp;

    private string _componentAddress;
    private string _componentAddressHex;

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
            newGlobalEntities.GetOrAdd(new GlobalEntityId(
                ((ComponentInfoSubstate)_componentInfoSubstate!.ActualInstance).EntityType,
                AddressHelper.AddressToHex(_componentAddress),
                AddressHelper.AddressToHex(_componentAddress),
                _componentAddress));
        }

        if (_componentStateSubstate != null)
        {
            newGlobalEntities.GetOrAdd(new GlobalEntityId(
                ((ComponentStateSubstate)_componentStateSubstate!.ActualInstance).EntityType,
                AddressHelper.AddressToHex(_componentAddress),
                AddressHelper.AddressToHex(_componentAddress),
                _componentAddress));
        }

        if (_componentSystemSubstate != null)
        {
            newGlobalEntities.GetOrAdd(new GlobalEntityId(
                ((SystemSubstate)_componentSystemSubstate!.ActualInstance).EntityType,
                AddressHelper.AddressToHex(_componentAddress),
                AddressHelper.AddressToHex(_componentAddress),
                _componentAddress));
        }

        var upSubstates = new List<UpSubstate>();

        if (_componentInfoSubstate != null)
        {
            upSubstates.Add(
                new UpSubstate(
                    new SubstateId(
                        ((ComponentInfoSubstate)_componentInfoSubstate!.ActualInstance).EntityType,
                        AddressHelper.AddressToHex(_componentAddress),
                        ((ComponentInfoSubstate)_componentInfoSubstate!.ActualInstance).SubstateType,
                        "00"
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
                    new SubstateId(
                        ((ComponentStateSubstate)_componentStateSubstate!.ActualInstance).EntityType,
                        AddressHelper.AddressToHex(_componentAddress),
                        ((ComponentStateSubstate)_componentStateSubstate!.ActualInstance).SubstateType,
                        "00"
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
                    new SubstateId(
                        ((SystemSubstate)_componentSystemSubstate!.ActualInstance).EntityType,
                        AddressHelper.AddressToHex(_componentAddress),
                        ((SystemSubstate)_componentSystemSubstate!.ActualInstance).SubstateType,
                        "00"
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
            new ComponentInfoSubstate(
                EntityType.Component,
                SubstateType.ComponentInfo,
                packageAddress,
                blueprintName
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
