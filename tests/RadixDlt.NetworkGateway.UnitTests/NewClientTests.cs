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

using FluentAssertions;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Abstractions.Serialization;
using Microsoft.Kiota.Http.HttpClientLibrary;
using Microsoft.Kiota.Serialization.Json;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Xunit;
using GHC = RadixDlt.CoreApiSdk.GenericHost.Client;
using GHM = RadixDlt.CoreApiSdk.GenericHost.Model;
using K = RadixDlt.CoreApiSdk.Kiota;
using KM = RadixDlt.CoreApiSdk.Kiota.Models;

namespace RadixDlt.NetworkGateway.UnitTests;

public class NewClientTests
{
    private const string DeserJsonAllProps = """{"substate_type":"AccessControllerFieldState","is_locked":false,"value":{"recovery_role_recovery_attempt":null,"timed_recovery_delay_minutes":0,"primary_role_recovery_attempt":null,"controlled_vault":{"entity_address":"entity_address","entity_type":"GlobalAccessController","is_global":true},"has_primary_role_badge_withdraw_attempt":false,"has_recovery_role_badge_withdraw_attempt":false,"is_primary_role_locked":false,"recovery_badge_resource_address":"some_address"}}""";
    private const string DeserJsonNoOptionalProperties = """{"substate_type":"AccessControllerFieldState","is_locked":false,"value":{"recovery_role_recovery_attempt":null,"primary_role_recovery_attempt":null,"controlled_vault":{"entity_address":"entity_address","entity_type":"GlobalAccessController","is_global":true},"has_primary_role_badge_withdraw_attempt":false,"has_recovery_role_badge_withdraw_attempt":false,"is_primary_role_locked":false,"recovery_badge_resource_address":"some_address"}}""";

    [Fact]
    public void GH_SerializeAccessControllerStateSubstate()
    {
        GHM.Substate substate = new GHM.AccessControllerFieldStateSubstate(
            isLocked: false,
            substateType: GHM.SubstateType.AccessControllerFieldState,
            value: new GHM.AccessControllerFieldStateValue(
                controlledVault: new GHM.EntityReference("entity_address", GHM.EntityType.GlobalAccessController, true),
                hasPrimaryRoleBadgeWithdrawAttempt: default,
                hasRecoveryRoleBadgeWithdrawAttempt: default,
                isPrimaryRoleLocked: false,
                recoveryBadgeResourceAddress: "some_address",
                primaryRoleRecoveryAttempt: new GHC.Option<GHM.PrimaryRoleRecoveryAttempt?>(),
                recoveryRoleRecoveryAttempt: new GHC.Option<GHM.RecoveryRoleRecoveryAttempt?>(),
                timedRecoveryDelayMinutes: new GHC.Option<long?>()));

        var options = new JsonSerializerOptions
        {
            Converters =
            {
                new JsonStringEnumConverter(),
            },
        };
        var json = JsonSerializer.Serialize(substate, options);

/*

broken polymorphism! :(

json = {
     "substate_type": "AccessControllerFieldState",
     "is_locked": false
   }

 */

        json.Should().Be("different");
    }

    [Fact]
    public void GH_DeserializeAccessControllerStateSubstate()
    {
        var options = new JsonSerializerOptions
        {
            Converters =
            {
                new JsonStringEnumConverter(),
            },
        };
        var result = JsonSerializer.Deserialize<GHM.Substate>(DeserJsonAllProps, options);

        // broken polymorphism! :(

        result.Should().Be("different");
    }

    [Fact]
    public async Task K_SerializeAccessControllerStateSubstate()
    {
        KM.Substate substate = new KM.AccessControllerFieldStateSubstate
        {
            SubstateType = KM.SubstateType.AccessControllerFieldState,
            IsLocked = false,
            Value = new KM.AccessControllerFieldStateValue
            {
                ControlledVault = new KM.EntityReference
                {
                    EntityAddress = "entity_address",
                    EntityType = KM.EntityType.GlobalAccessController,
                    IsGlobal = true,
                },
                HasPrimaryRoleBadgeWithdrawAttempt = false,
                HasRecoveryRoleBadgeWithdrawAttempt = false,
                IsPrimaryRoleLocked = false,
                RecoveryBadgeResourceAddress = "some_address",
                PrimaryRoleRecoveryAttempt = default,
                RecoveryRoleRecoveryAttempt = default,
                TimedRecoveryDelayMinutes = default,
            },
        };

        var authProvider = new AnonymousAuthenticationProvider();
        var adapter = new HttpClientRequestAdapter(authProvider);
        var client = new K.MyClass(adapter); // needed to setup writer factories in adapter

        using var writer = adapter.SerializationWriterFactory.GetSerializationWriter("application/json");
        writer.WriteObjectValue(null, substate);
        using var writerReader = new StreamReader(writer.GetSerializedContent());
        var json = await writerReader.ReadToEndAsync();

        /*

ALL GOOD! albeit 'substate_type' isn't a very first property which MAY be problematic for some JSON libraries that rely on its position (such as C#'s System.Text.Json) - then again our current setup also violates this rule

json = {
             "is_locked": false,
             "substate_type": "AccessControllerFieldState",
             "value": {
               "recovery_role_recovery_attempt": null,
               "timed_recovery_delay_minutes": null,
               "primary_role_recovery_attempt": null,
               "controlled_vault": {
                 "entity_address": "entity_address",
                 "entity_type": "GlobalAccessController",
                 "is_global": true
               },
               "has_primary_role_badge_withdraw_attempt": false,
               "has_recovery_role_badge_withdraw_attempt": false,
               "is_primary_role_locked": false,
               "recovery_badge_resource_address": "some_address"
             }
           }

         */

        json.Should().Be("different");
    }

    [Fact]
    public void K_DeserializeAccessControllerStateSubstate()
    {
        var authProvider = new AnonymousAuthenticationProvider();
        var adapter = new HttpClientRequestAdapter(authProvider);
        var client = new K.MyClass(adapter); // needed to setup writer factories in adapter

        var ss = new MemoryStream(Encoding.UTF8.GetBytes(DeserJsonAllProps));
        var parseNode = ParseNodeFactoryRegistry.DefaultInstance.GetRootParseNode("application/json", ss);

        var result = parseNode.GetObjectValue<KM.Substate>(KM.Substate.CreateFromDiscriminatorValue);

        // all good!

        result.Should().Be("different");
    }

    [Fact]
    public void K_DeserializeAccessControllerStateSubstate_NoOptionalProps()
    {
        var authProvider = new AnonymousAuthenticationProvider();
        var adapter = new HttpClientRequestAdapter(authProvider);
        var client = new K.MyClass(adapter); // needed to setup writer factories in adapter

        var ss = new MemoryStream(Encoding.UTF8.GetBytes(DeserJsonNoOptionalProperties));
        var parseNode = ParseNodeFactoryRegistry.DefaultInstance.GetRootParseNode("application/json", ss);

        var result = parseNode.GetObjectValue<KM.Substate>(KM.Substate.CreateFromDiscriminatorValue);

        // all good!

        result.Should().Be("different");
    }

    [Fact]
    public async Task K_Deser_Ser_Deser()
    {
        var authProvider = new AnonymousAuthenticationProvider();
        var adapter = new HttpClientRequestAdapter(authProvider);
        var client = new K.MyClass(adapter); // needed to setup writer factories in adapter

        var ss1 = new MemoryStream(Encoding.UTF8.GetBytes(DeserJsonNoOptionalProperties));
        var parseNode1 = ParseNodeFactoryRegistry.DefaultInstance.GetRootParseNode("application/json", ss1);
        var firstDeserResult = parseNode1.GetObjectValue<KM.Substate>(KM.Substate.CreateFromDiscriminatorValue);

        // we cannot use using var writer = adapter.SerializationWriterFactory.GetSerializationWriter("application/json");
        // as that serializer comes with OnBeforeSerialization hooks that modify value's BackingStore to return only modified properties
        // see: BackingStoreSerializationWriterProxyFactory's ctor
        var writer = new JsonSerializationWriter();
        writer.WriteObjectValue(null, firstDeserResult);
        using var writerReader = new StreamReader(writer.GetSerializedContent());
        var serResult = await writerReader.ReadToEndAsync();

        var ss2 = new MemoryStream(Encoding.UTF8.GetBytes(serResult));
        var parseNode2 = ParseNodeFactoryRegistry.DefaultInstance.GetRootParseNode("application/json", ss2);
        var secondDeserResult = parseNode2.GetObjectValue<KM.Substate>(KM.Substate.CreateFromDiscriminatorValue);

        // all good!

        firstDeserResult.Should().BeEquivalentTo(secondDeserResult);
    }

    [Fact]
    public void GH_Discriminator()
    {
        var opts = new JsonSerializerOptions
        {
            Converters =
            {
                new JsonStringEnumConverter(),
                new GHM.MyDiscriminatorTestJsonConverter(),
            },
        };

        var myDiscriminator = new GHM.MyDiscriminatorTestVariantA(GHM.MyDiscriminatorType.VariantA, new GHC.Option<string?>("some_Val"));
        var json = JsonSerializer.Serialize(myDiscriminator, opts);
        var deser = JsonSerializer.Deserialize<GHM.MyDiscriminatorTest>("""{"type":"VariantA","prop_a": "some_Val"}""", opts);

        json.Should().Be("different");
        deser.Should().Be("different");
    }
}
