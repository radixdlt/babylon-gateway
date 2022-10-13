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
