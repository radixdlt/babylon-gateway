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

using Newtonsoft.Json;
using Riok.Mapperly.Abstractions;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CoreModel = RadixDlt.CoreApiSdk.Model;
using GatewayModel = RadixDlt.NetworkGateway.GatewayApiSdk.Model;

namespace RadixDlt.NetworkGateway.CoreApiMapping;

[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName, EnumMappingIgnoreCase = true)]
public static partial class CoreModelMapping
{
    private static readonly JsonSerializerSettings _serializerSettings = new()
    {
        ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
    };

    public static partial GatewayModel.CaTransactionReceipt ToGatewayCaModel(this CoreModel.TransactionReceipt source);

    public static partial List<GatewayModel.CaInstructionResourceChanges> ToGatewayCaModel(this List<CoreModel.InstructionResourceChanges> source);

    public static GatewayModel.CaAuthConfig? CaAuthConfig(string? raw)
    {
        return TryDeserialize<CoreModel.AuthConfig>(raw, out var source)
            ? source.ToGatewayCaModel()
            : null;
    }

    public static GatewayModel.CaFeeSummary? CaFeeSummary(string? raw)
    {
        return TryDeserialize<CoreModel.FeeSummary>(raw, out var source)
            ? source.ToGatewayCaModel()
            : null;
    }

    public static GatewayModel.CaCostingParameters? CaCostingParameters(string? raw)
    {
        return TryDeserialize<CoreModel.CostingParameters>(raw, out var source)
            ? source.ToGatewayCaModel()
            : null;
    }

    public static GatewayModel.CaFeeSource? CaFeeSource(string? raw)
    {
        return TryDeserialize<CoreModel.FeeSource>(raw, out var source)
            ? source.ToGatewayCaModel()
            : null;
    }

    public static GatewayModel.CaFeeDestination? CaFeeDestination(string? raw)
    {
        return TryDeserialize<CoreModel.FeeDestination>(raw, out var source)
            ? source.ToGatewayCaModel()
            : null;
    }

    public static GatewayModel.CaStateUpdates?CaStateUpdates(string? raw)
    {
        return TryDeserialize<CoreModel.StateUpdates>(raw, out var source)
            ? source.ToGatewayCaModel()
            : null;
    }

    public static GatewayModel.CaNextEpoch? CaNextEpoch(string? raw)
    {
        return TryDeserialize<CoreModel.NextEpoch>(raw, out var source)
            ? source.ToGatewayCaModel()
            : null;
    }

    public static List<GatewayModel.CaSborData>? CaListSborData(string? raw)
    {
        return TryDeserialize<List<CoreModel.SborData>>(raw, out var source)
            ? source.ToGatewayCaModel()
            : null;
    }

    public static GatewayModel.CaEventEmitterIdentifier? CaEventEmitterIdentifier(string? raw)
    {
        return TryDeserialize<CoreModel.EventEmitterIdentifier>(raw, out var source)
            ? source.ToGatewayCaModel()
            : null;
    }

    public static GatewayModel.CaTransactionMessage? CaTransactionMessage(string? raw)
    {
        return TryDeserialize<CoreModel.TransactionMessage>(raw, out var source)
            ? source.ToGatewayCaModel()
            : null;
    }

    public static GatewayModel.CaBlueprintDefinition? CaBlueprintDefinition(string? raw)
    {
        return TryDeserialize<CoreModel.BlueprintDefinition>(raw, out var source)
            ? source.ToGatewayCaModel()
            : null;
    }

    public static GatewayModel.CaValidatorFieldStateValue? CaValidatorFieldStateValue(string? raw)
    {
        return TryDeserialize<CoreModel.ValidatorFieldStateValue>(raw, out var source)
            ? source.ToGatewayCaModel()
            : null;
    }

    public static GatewayModel.CaOwnerRole? CaOwnerRole(string? raw)
    {
        return TryDeserialize<CoreModel.OwnerRole>(raw, out var source)
            ? source.ToGatewayCaModel()
            : null;
    }

    public static GatewayModel.CaAccessRule? CaAccessRule(string? raw)
    {
        return TryDeserialize<CoreModel.AccessRule>(raw, out var source)
            ? source.ToGatewayCaModel()
            : null;
    }

    public static GatewayModel.CaRoyaltyAmount? CaRoyaltyAmount(string? raw)
    {
        return TryDeserialize<CoreModel.RoyaltyAmount>(raw, out var source)
            ? source.ToGatewayCaModel()
            : null;
    }

    public static GatewayModel.CaBlueprintRoyaltyConfig? CaBlueprintRoyaltyConfig(string? raw)
    {
        return TryDeserialize<CoreModel.BlueprintRoyaltyConfig>(raw, out var source)
            ? source.ToGatewayCaModel()
            : null;
    }

    private static bool TryDeserialize<T>(string? raw, [MaybeNullWhen(false)] out T deserialized)
        where T : class
    {
        deserialized = raw == null
            ? null
            : JsonConvert.DeserializeObject<T>(raw, _serializerSettings);

        return deserialized != null;
    }

    // TODO remove everything below once Riok.Mapperly issue with nullable #enable and Dictionary<string?, ...> gets fixed

    [UserMapping(Default = true)]
    private static GatewayModel.CaBlueprintDefinition ToGatewayCaModel(this CoreModel.BlueprintDefinition source)
    {
        return new GatewayModel.CaBlueprintDefinition(
            _interface: source.Interface.ToGatewayCaModel(),
            functionExports: source.FunctionExports.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToGatewayCaModel()),
            hookExports: source.HookExports.Select(x => x.ToGatewayCaModel()).ToList());
    }

    [UserMapping(Default = true)]
    private static GatewayModel.CaBlueprintInterface ToGatewayCaModel(this CoreModel.BlueprintInterface source)
    {
        return new GatewayModel.CaBlueprintInterface(
            outerBlueprint: source.OuterBlueprint,
            genericTypeParameters: source.GenericTypeParameters.Select(x => x.ToGatewayCaModel()).ToList(),
            isTransient: source.IsTransient,
            features: source.Features,
            state: source.State.ToGatewayCaModel(),
            functions: source.Functions.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToGatewayCaModel()),
            events: source.Events.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToGatewayCaModel()),
            types: source.Types.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToGatewayCaModel()));
    }

    [UserMapping(Default = true)]
    private static GatewayModel.CaAuthConfig ToGatewayCaModel(this CoreModel.AuthConfig source)
    {
        return new GatewayModel.CaAuthConfig(
            functionAuthType: source.FunctionAuthType.ToGatewayCaModel(),
            functionAccessRules: source.FunctionAccessRules.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToGatewayCaModel()),
            methodAuthType: source.MethodAuthType.ToGatewayCaModel(),
            methodRoles: source.MethodRoles.ToGatewayCaModel());
    }

    [UserMapping(Default = true)]
    private static GatewayModel.CaStaticRoleDefinitionAuthTemplate ToGatewayCaModel(this CoreModel.StaticRoleDefinitionAuthTemplate source)
    {
        return new GatewayModel.CaStaticRoleDefinitionAuthTemplate(
            roleSpecification: source.RoleSpecification.ToGatewayCaModel(),
            roles: source.Roles.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToGatewayCaModel()),
            methodAccessibilityMap: source.MethodAccessibilityMap.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToGatewayCaModel()));
    }

    private static partial GatewayModel.CaRoleSpecification ToGatewayCaModel(this CoreModel.RoleSpecification source);

    private static partial GatewayModel.CaFunctionAuthType ToGatewayCaModel(this CoreModel.FunctionAuthType source);

    private static partial GatewayModel.CaMethodAuthType ToGatewayCaModel(this CoreModel.MethodAuthType source);

    private static partial GatewayModel.CaMethodAccessibility ToGatewayCaModel(this CoreModel.MethodAccessibility source);

    private static partial GatewayModel.CaRoleDetails ToGatewayCaModel(this CoreModel.RoleDetails source);

    private static partial GatewayModel.CaFeeSummary ToGatewayCaModel(this CoreModel.FeeSummary source);

    private static partial GatewayModel.CaCostingParameters ToGatewayCaModel(this CoreModel.CostingParameters source);

    private static partial GatewayModel.CaFeeSource ToGatewayCaModel(this CoreModel.FeeSource source);

    private static partial GatewayModel.CaFeeDestination ToGatewayCaModel(this CoreModel.FeeDestination source);

    private static partial GatewayModel.CaStateUpdates ToGatewayCaModel(this CoreModel.StateUpdates source);

    private static partial GatewayModel.CaNextEpoch ToGatewayCaModel(this CoreModel.NextEpoch source);

    private static partial List<GatewayModel.CaSborData> ToGatewayCaModel(this List<CoreModel.SborData> source);

    private static partial GatewayModel.CaEventEmitterIdentifier ToGatewayCaModel(this CoreModel.EventEmitterIdentifier source);

    private static partial GatewayModel.CaTransactionMessage ToGatewayCaModel(this CoreModel.TransactionMessage source);

    private static partial GatewayModel.CaRoyaltyAmount ToGatewayCaModel(this CoreModel.RoyaltyAmount source);

    private static partial GatewayModel.CaBlueprintRoyaltyConfig ToGatewayCaModel(this CoreModel.BlueprintRoyaltyConfig source);

    private static partial GatewayModel.CaValidatorFieldStateValue ToGatewayCaModel(this CoreModel.ValidatorFieldStateValue source);

    private static partial GatewayModel.CaAccessRule ToGatewayCaModel(this CoreModel.AccessRule source);

    private static partial GatewayModel.CaOwnerRole ToGatewayCaModel(this CoreModel.OwnerRole source);

    private static partial GatewayModel.CaPackageExport ToGatewayCaModel(this CoreModel.PackageExport source);

    private static partial GatewayModel.CaGenericTypeParameter ToGatewayCaModel(this CoreModel.GenericTypeParameter source);

    private static partial GatewayModel.CaIndexedStateSchema ToGatewayCaModel(this CoreModel.IndexedStateSchema source);

    private static partial GatewayModel.CaFunctionSchema ToGatewayCaModel(this CoreModel.FunctionSchema source);

    private static partial GatewayModel.CaBlueprintPayloadDef ToGatewayCaModel(this CoreModel.BlueprintPayloadDef source);

    private static partial GatewayModel.CaScopedTypeId ToGatewayCaModel(this CoreModel.ScopedTypeId source);

    private static partial GatewayModel.CaHookExport ToGatewayCaModel(this CoreModel.HookExport source);
}
