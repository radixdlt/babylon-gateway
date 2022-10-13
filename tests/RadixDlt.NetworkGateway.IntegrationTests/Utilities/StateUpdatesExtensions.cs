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
using RadixDlt.NetworkGateway.IntegrationTests.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RadixDlt.NetworkGateway.IntegrationTests.Utilities;

public static class StateUpdatesExtensions
{
    public static StateUpdates Add(this StateUpdates stateUpdates1, StateUpdates stateUpdates2)
    {
        stateUpdates1.DownVirtualSubstates.AddRange(stateUpdates2.DownVirtualSubstates);
        stateUpdates1.UpSubstates.AddRange(stateUpdates2.UpSubstates);
        stateUpdates1.DownSubstates.AddRange(stateUpdates2.DownSubstates);
        stateUpdates1.NewGlobalEntities.AddRange(stateUpdates2.NewGlobalEntities);

        return stateUpdates1;
    }

    public static StateUpdates Combine(this List<StateUpdates> stateUpdatesList)
    {
        return new StateUpdates(
            stateUpdatesList.SelectMany(s => s.DownVirtualSubstates).ToList(),
            stateUpdatesList.SelectMany(s => s.UpSubstates).ToList(),
            stateUpdatesList.SelectMany(s => s.DownSubstates).ToList(),
            stateUpdatesList.SelectMany(s => s.NewGlobalEntities).ToList());
    }

    public static string ToJson(this List<StateUpdates> stateUpdatesList)
    {
        return Combine(stateUpdatesList).ToJson();
    }

    public static GlobalEntityId GetGlobalEntity(this StateUpdates stateUpdates, string globalAddress)
    {
        return stateUpdates.NewGlobalEntities.Find(ge => ge.GlobalAddress == globalAddress)!;
    }

    public static UpSubstate GetLastUpSubstateByEntityAddress(this StateUpdates stateUpdates, string entityAddress)
    {
        var entityAddressHex = stateUpdates.NewGlobalEntities.FindLast(ge => ge.GlobalAddress == entityAddress)!.EntityAddressHex;

        return stateUpdates.GetLastUpSubstateByEntityAddressHex(entityAddressHex);
    }

    public static UpSubstate GetLastUpSubstateByEntityAddressHex(this StateUpdates stateUpdates, string? entityAddressHex)
    {
        return stateUpdates.UpSubstates.FindLast(us => us.SubstateId.EntityAddressHex == entityAddressHex)!;
    }

    public static DownSubstate GetLastDownSubstateByEntityAddress(this StateUpdates stateUpdates, string entityAddress)
    {
        var entityAddressHex = stateUpdates.NewGlobalEntities.FindLast(ge => ge.GlobalAddress == entityAddress)!.EntityAddressHex;

        return stateUpdates.GetLastDownSubstateByEntityAddressHex(entityAddressHex);
    }

    public static DownSubstate GetLastDownSubstateByEntityAddressHex(this StateUpdates stateUpdates, string? entityAddressHex)
    {
        return stateUpdates.DownSubstates.FindLast(us => us.SubstateId.EntityAddressHex == entityAddressHex)!;
    }

    public static UpSubstate GetLastVaultUpSubstateByEntityAddress(this StateUpdates stateUpdates, string entityAddress)
    {
        var componentUpSubstate = stateUpdates.GetLastUpSubstateByEntityAddress(entityAddress);

        var vaultEntityAddressHex = (componentUpSubstate.SubstateData.ActualInstance as ComponentStateSubstate)?.OwnedEntities.First(v => v.EntityType == EntityType.Vault)
            .EntityAddressHex;

        return stateUpdates.GetLastUpSubstateByEntityAddressHex(vaultEntityAddressHex);
    }

    public static string? GetVaultAddressHexByEntityAddress(this StateUpdates stateUpdates, string entityAddress)
    {
        var componentUpSubstate = stateUpdates.GetLastUpSubstateByEntityAddress(entityAddress);

        return (componentUpSubstate.SubstateData.ActualInstance as ComponentStateSubstate)?.OwnedEntities.First(v => v.EntityType == EntityType.Vault).EntityAddressHex;
    }

    public static DownSubstate? GetLastVaultDownSubstateByEntityAddress(this StateUpdates stateUpdates, string entityAddress)
    {
        var vaultEntityAddressHex = GetVaultAddressHexByEntityAddress(stateUpdates, entityAddress);

        return stateUpdates.DownSubstates.FirstOrDefault(ds => ds.SubstateId.EntityAddressHex == vaultEntityAddressHex);
    }

    public static ResourceManagerSubstate GetFungibleResourceUpSubstateByEntityAddress(this StateUpdates stateUpdates, string entityAddress)
    {
        var resourceUpSubstate = stateUpdates.GetLastUpSubstateByEntityAddress(entityAddress);

        return (resourceUpSubstate.SubstateData.ActualInstance as ResourceManagerSubstate)!;
    }

    public static int GetFungibleResourceDivisibilityEntityAddress(this StateUpdates stateUpdates, string entityAddress)
    {
        var vaultUpSubstate = stateUpdates.GetLastVaultUpSubstateByEntityAddress(entityAddress);

        var vaultSubstate = vaultUpSubstate.SubstateData.GetVaultSubstate();

        var xrdResourceAddress = vaultSubstate.PointedResources.First();

        var resourceUpSubstate = stateUpdates.GetLastUpSubstateByEntityAddressHex(xrdResourceAddress.Address);

        var resourceManagerSubstate = resourceUpSubstate.SubstateData.ActualInstance as ResourceManagerSubstate;

        return resourceManagerSubstate!.FungibleDivisibility;
    }

    public static string GetFungibleResoureAddressByEntityAddress(this StateUpdates stateUpdates, string entityAddress)
    {
        var vaultUpSubstate = stateUpdates.GetLastVaultUpSubstateByEntityAddress(entityAddress);

        var vaultSubstate = vaultUpSubstate.SubstateData.GetVaultSubstate();

        var xrdResourceAddress = vaultSubstate.PointedResources.First();

        return xrdResourceAddress.Address;
    }

    public static GlobalEntityId GetOrAdd(this List<GlobalEntityId> source, GlobalEntityId item)
    {
        var globalEntityId = source.Find(ge => ge.GlobalAddress == item.GlobalAddress &&
                                               ge.GlobalAddressHex == item.GlobalAddressHex);

        if (globalEntityId != null)
        {
            return globalEntityId;
        }

        source.Add(item);

        return item;
    }

    /// <summary>
    ///     Withdraws a given amount of tokens form the vault, creates new vault's down and up  substates, and updates vault balance (ResourceAmount.AmountAttos).
    /// </summary>
    /// <param name="stateUpdates">state updates.</param>
    /// <param name="componentAddress">vault owned by component address.</param>
    /// <param name="feeSummary">fee summary.</param>
    /// <param name="tokensToWithdraw">amount of tokens to withdraw.</param>
    /// <param name="newVaultBalanceAttos">new vault balance in attos.</param>
    /// <returns>updated state updates.</returns>
    public static StateUpdates TakeTokensFromVault(
            this StateUpdates stateUpdates,
            string componentAddress,
            FeeSummary feeSummary,
            double tokensToWithdraw,
            out string newVaultBalanceAttos)
    {
        var attosToWithdraw = TokenAttosConverter.Tokens2Attos(tokensToWithdraw);

        // get xrd resource address
        var resourceAddress = GetFungibleResoureAddressByEntityAddress(stateUpdates, componentAddress);

        // get last upsubstate of the vault
        var vaultUpSubstate = GetLastVaultUpSubstateByEntityAddress(stateUpdates, componentAddress);

        var vaultAddressHex = vaultUpSubstate.SubstateId.EntityAddressHex;

        var vaultSubstate = vaultUpSubstate.SubstateData.GetVaultSubstate();

        // get vault's balance
        var vaultResourceAmount = vaultSubstate.ResourceAmount.GetFungibleResourceAmount();
        var vaultResourceAmountAttos = TokenAttosConverter.ParseAttosFromString(vaultResourceAmount.AmountAttos);

        // withdraw 'attosToWithdraw' and fees
        var feesAttos = feeSummary.CostUnitConsumed
                        * TokenAttosConverter.ParseAttosFromString(feeSummary.CostUnitPriceAttos);

        newVaultBalanceAttos = (vaultResourceAmountAttos - attosToWithdraw - feesAttos).ToString();

        // build vault's new down and up substates
        var vault = new VaultBuilder()
            .WithFixedAddressHex(vaultAddressHex)
            .WithFungibleTokensResourceAddress(resourceAddress)
            .WithFungibleResourceAmountAttos(newVaultBalanceAttos)
            .WithDownState(new DownSubstate(
                new SubstateId(
                    EntityType.Vault,
                    vaultAddressHex,
                    SubstateType.Vault,
                    Convert.ToHexString(Encoding.UTF8.GetBytes("substateKeyHex")).ToLowerInvariant()
                ), "hash", vaultUpSubstate._Version)
            ).Build();

        return vault;
    }
}
