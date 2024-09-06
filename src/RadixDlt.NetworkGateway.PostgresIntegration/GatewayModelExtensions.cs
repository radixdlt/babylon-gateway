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
using Newtonsoft.Json.Linq;
using RadixDlt.NetworkGateway.Abstractions;
using RadixDlt.NetworkGateway.Abstractions.Extensions;
using RadixDlt.NetworkGateway.Abstractions.Model;
using RadixDlt.NetworkGateway.Abstractions.Numerics;
using RadixDlt.NetworkGateway.Abstractions.StandardMetadata;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using RadixDlt.NetworkGateway.PostgresIntegration.Queries;
using RadixDlt.NetworkGateway.PostgresIntegration.Queries.CustomTypes;
using RadixDlt.NetworkGateway.PostgresIntegration.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CoreModel = RadixDlt.CoreApiSdk.Model;
using GatewayModel = RadixDlt.NetworkGateway.GatewayApiSdk.Model;
using LedgerTransaction = RadixDlt.NetworkGateway.PostgresIntegration.Models.LedgerTransaction;
using NonFungibleIdType = RadixDlt.NetworkGateway.Abstractions.Model.NonFungibleIdType;
using PublicKeyType = RadixDlt.NetworkGateway.Abstractions.Model.PublicKeyType;
using ToolkitModel = RadixEngineToolkit;
using UserLedgerTransaction = RadixDlt.NetworkGateway.PostgresIntegration.Models.UserLedgerTransaction;

namespace RadixDlt.NetworkGateway.PostgresIntegration;

internal static class GatewayModelExtensions
{
    public static string? GenerateOffsetCursor(int offset, int limit, long totalCount)
    {
        return offset + limit < totalCount
            ? new GatewayModel.OffsetCursor(offset + limit).ToCursorString()
            : null;
    }

    public static void ToGatewayModel(
        this EntityResourcesQuery.PerEntityQueryResultRow input,
        bool aggregatePerVault,
        IDictionary<long, GatewayModel.EntityMetadataCollection>? explicitMetadata,
        IDictionary<long, GatewayModel.NonFungibleIdsCollection>? nfVaultContents,
        out GatewayModel.FungibleResourcesCollection fungibles,
        out GatewayModel.NonFungibleResourcesCollection nonFungibles)
    {
        var fungibleResourceNextCursorInclusive = input.FungibleResources.SingleOrDefault(x => x.ResourceNextCursorInclusive.HasValue)?.ResourceNextCursorInclusive;
        string? fungibleCursor = fungibleResourceNextCursorInclusive.ToGatewayModel()?.ToCursorString();

        var fungibleItems = input.FungibleResources
            .Where(x => !x.ResourceFilterOut)
            .Select(
                f =>
            {
                GatewayModel.FungibleResourcesCollectionItem val;
                GatewayModel.EntityMetadataCollection? resourceExplicitMetadata = null;

                explicitMetadata?.TryGetValue(f.ResourceEntityId, out resourceExplicitMetadata);

                if (aggregatePerVault)
                {
                    var vaultNextCursorInclusive = f.Vaults.SingleOrDefault(x => x.VaultNextCursorInclusive.HasValue)?.VaultNextCursorInclusive;
                    string? vaultCursor = vaultNextCursorInclusive.ToGatewayModel()?.ToCursorString();
                    var vaults = f.Vaults
                        .Where(x => !x.VaultFilterOut)
                        .Select(v => new GatewayModel.FungibleResourcesCollectionItemVaultAggregatedVaultItem(
                            vaultAddress: v.VaultEntityAddress,
                            amount: TokenAmount.FromSubUnitsString(v.VaultBalance).ToString(),
                            lastUpdatedAtStateVersion: v.VaultLastUpdatedAtStateVersion))
                        .ToList();

                    val = new GatewayModel.FungibleResourcesCollectionItemVaultAggregated(
                        resourceAddress: f.ResourceEntityAddress,
                        vaults: new GatewayModel.FungibleResourcesCollectionItemVaultAggregatedVault(
                            totalCount: f.ResourceVaultTotalCount,
                            nextCursor: vaultCursor,
                            items: vaults),
                        explicitMetadata: resourceExplicitMetadata);
                }
                else
                {
                    val = new GatewayModel.FungibleResourcesCollectionItemGloballyAggregated(
                        resourceAddress: f.ResourceEntityAddress,
                        amount: TokenAmount.FromSubUnitsString(f.ResourceBalance).ToString(),
                        lastUpdatedAtStateVersion: f.ResourceLastUpdatedAtStateVersion,
                        explicitMetadata: resourceExplicitMetadata);
                }

                return val;
            })
            .ToList();

        var nonFungibleResourceNextCursorInclusive = input.NonFungibleResources.SingleOrDefault(x => x.ResourceNextCursorInclusive.HasValue)?.ResourceNextCursorInclusive;
        string? nonFungibleCursor = nonFungibleResourceNextCursorInclusive.ToGatewayModel()?.ToCursorString();
        var nonFungibleItems = input.NonFungibleResources
            .Where(x => !x.ResourceFilterOut)
            .Select(
                nf =>
            {
                GatewayModel.NonFungibleResourcesCollectionItem val;
                GatewayModel.EntityMetadataCollection? resourceExplicitMetadata = null;

                explicitMetadata?.TryGetValue(nf.ResourceEntityId, out resourceExplicitMetadata);

                if (aggregatePerVault)
                {
                    var vaultNextCursorInclusive = nf.Vaults.SingleOrDefault(x => x.VaultNextCursorInclusive.HasValue)?.VaultNextCursorInclusive;
                    string? vaultCursor = vaultNextCursorInclusive.ToGatewayModel()?.ToCursorString();

                    var vaults = nf.Vaults
                        .Where(x => !x.VaultFilterOut)
                        .Select(
                            v =>
                            {
                                string? nfidCursor = null;
                                List<string>? nfids = null;

                                if (nfVaultContents?.TryGetValue(v.VaultEntityId, out var vaultNfids) == true)
                                {
                                    nfidCursor = vaultNfids.NextCursor;
                                    nfids = vaultNfids.Items;
                                }

                                return new GatewayModel.NonFungibleResourcesCollectionItemVaultAggregatedVaultItem(
                                    totalCount: long.Parse(TokenAmount.FromSubUnitsString(v.VaultBalance).ToString()),
                                    nextCursor: nfidCursor,
                                    items: nfids,
                                    vaultAddress: v.VaultEntityAddress,
                                    lastUpdatedAtStateVersion: v.VaultLastUpdatedAtStateVersion);
                        })
                        .ToList();

                    val = new GatewayModel.NonFungibleResourcesCollectionItemVaultAggregated(
                        resourceAddress: nf.ResourceEntityAddress,
                        vaults: new GatewayModel.NonFungibleResourcesCollectionItemVaultAggregatedVault(
                            totalCount: nf.ResourceVaultTotalCount,
                            nextCursor: vaultCursor,
                            items: vaults),
                        explicitMetadata: resourceExplicitMetadata);
                }
                else
                {
                    val = new GatewayModel.NonFungibleResourcesCollectionItemGloballyAggregated(
                        resourceAddress: nf.ResourceEntityAddress,
                        amount: long.Parse(TokenAmount.FromSubUnitsString(nf.ResourceBalance).ToString()),
                        lastUpdatedAtStateVersion: nf.ResourceLastUpdatedAtStateVersion,
                        explicitMetadata: resourceExplicitMetadata);
                }

                return val;
            })
            .ToList();

        fungibles = new GatewayModel.FungibleResourcesCollection(input.TotalFungibleResourceCount, fungibleCursor, fungibleItems);
        nonFungibles = new GatewayModel.NonFungibleResourcesCollection(input.TotalNonFungibleResourceCount, nonFungibleCursor, nonFungibleItems);
    }

    public static GatewayModel.IdBoundaryCoursor? ToGatewayModel(this IdBoundaryCursor? input)
    {
        return input == null
            ? null
            : new GatewayModel.IdBoundaryCoursor(input.Value.StateVersion, input.Value.Id);
    }

    public static IdBoundaryCursor? FromGatewayModel(this GatewayModel.IdBoundaryCoursor? input)
    {
        return input != null
            ? new IdBoundaryCursor(input.StateVersionBoundary, input.IdBoundary)
            : null;
    }

    public static GatewayModel.TwoWayLinkedDappsCollectionItem ToGatewayModel(this DappDefinitionsResolvedTwoWayLink input)
    {
        return new GatewayModel.TwoWayLinkedDappsCollectionItem(input.EntityAddress);
    }

    public static GatewayModel.TwoWayLinkedEntitiesCollectionItem ToGatewayModel(this DappClaimedEntityResolvedTwoWayLink input)
    {
        return new GatewayModel.TwoWayLinkedEntitiesCollectionItem(input.EntityAddress);
    }

    public static GatewayModel.AccountDefaultDepositRule ToGatewayModel(this AccountDefaultDepositRule input)
    {
        return input switch
        {
            AccountDefaultDepositRule.Accept => GatewayModel.AccountDefaultDepositRule.Accept,
            AccountDefaultDepositRule.Reject => GatewayModel.AccountDefaultDepositRule.Reject,
            AccountDefaultDepositRule.AllowExisting => GatewayModel.AccountDefaultDepositRule.AllowExisting,
            _ => throw new ArgumentOutOfRangeException(nameof(input), input, null),
        };
    }

    public static GatewayModel.AccountResourcePreferenceRule ToGatewayModel(this AccountResourcePreferenceRule input)
    {
        return input switch
        {
            AccountResourcePreferenceRule.Allowed => GatewayModel.AccountResourcePreferenceRule.Allowed,
            AccountResourcePreferenceRule.Disallowed => GatewayModel.AccountResourcePreferenceRule.Disallowed,
            _ => throw new ArgumentOutOfRangeException(nameof(input), input, null),
        };
    }

    public static GatewayModel.ManifestClass ToGatewayModel(this LedgerTransactionManifestClass input)
    {
        return input switch
        {
            LedgerTransactionManifestClass.General => GatewayModel.ManifestClass.General,
            LedgerTransactionManifestClass.Transfer => GatewayModel.ManifestClass.Transfer,
            LedgerTransactionManifestClass.ValidatorStake => GatewayModel.ManifestClass.ValidatorStake,
            LedgerTransactionManifestClass.ValidatorUnstake => GatewayModel.ManifestClass.ValidatorUnstake,
            LedgerTransactionManifestClass.ValidatorClaim => GatewayModel.ManifestClass.ValidatorClaim,
            LedgerTransactionManifestClass.AccountDepositSettingsUpdate => GatewayModel.ManifestClass.AccountDepositSettingsUpdate,
            LedgerTransactionManifestClass.PoolContribution => GatewayModel.ManifestClass.PoolContribution,
            LedgerTransactionManifestClass.PoolRedemption => GatewayModel.ManifestClass.PoolRedemption,
            _ => throw new ArgumentOutOfRangeException(nameof(input), input, null),
        };
    }

    public static GatewayModel.NonFungibleIdType ToGatewayModel(this NonFungibleIdType nonFungibleIdType)
    {
        return nonFungibleIdType switch
        {
            NonFungibleIdType.String => GatewayModel.NonFungibleIdType.String,
            NonFungibleIdType.Integer => GatewayModel.NonFungibleIdType.Integer,
            NonFungibleIdType.Bytes => GatewayModel.NonFungibleIdType.Bytes,
            NonFungibleIdType.RUID => GatewayModel.NonFungibleIdType.Ruid,
            _ => throw new ArgumentOutOfRangeException(nameof(nonFungibleIdType), nonFungibleIdType, null),
        };
    }

    public static GatewayModel.PublicKey ToGatewayPublicKey(this ValidatorPublicKeyHistory validatorPublicKey)
    {
        var keyHex = validatorPublicKey.Key.ToHex();

        return validatorPublicKey.KeyType switch
        {
            PublicKeyType.EcdsaSecp256k1 => new GatewayModel.PublicKeyEcdsaSecp256k1(keyHex),
            PublicKeyType.EddsaEd25519 => new GatewayModel.PublicKeyEddsaEd25519(keyHex),
            _ => throw new UnreachableException($"Didn't expect {validatorPublicKey.KeyType} value"),
        };
    }

    public static GatewayModel.CommittedTransactionInfo ToGatewayModel(
        this LedgerTransaction lt,
        GatewayModel.TransactionDetailsOptIns optIns,
        Dictionary<long, string> entityIdToAddressMap,
        List<TransactionQuerier.Event>? events,
        GatewayModel.TransactionBalanceChanges? transactionBalanceChanges)
    {
        string? payloadHash = null;
        string? intentHash = null;
        string? rawHex = null;
        JRaw? message = null;
        string? manifestInstructions = null;
        List<GatewayModel.ManifestClass>? manifestClasses = null;

        if (lt is UserLedgerTransaction ult)
        {
            payloadHash = ult.PayloadHash;
            intentHash = ult.IntentHash;
            rawHex = optIns.RawHex ? ult.RawPayload.ToHex() : null;
            message = ult.Message != null ? new JRaw(ult.Message) : null;
            manifestInstructions = optIns.ManifestInstructions ? ult.ManifestInstructions : null;
            manifestClasses = ult.ManifestClasses.Select(mc => mc.ToGatewayModel()).ToList();
        }

        var receipt = new GatewayModel.TransactionReceipt
        {
            ErrorMessage = lt.EngineReceipt.ErrorMessage,
            Status = ToGatewayModel(lt.EngineReceipt.Status),
            Output = optIns.ReceiptOutput && lt.EngineReceipt.Output != null ? new JRaw(lt.EngineReceipt.Output) : null,
            FeeSummary = optIns.ReceiptFeeSummary ? new JRaw(lt.EngineReceipt.FeeSummary) : null,
            FeeDestination = optIns.ReceiptFeeDestination && lt.EngineReceipt.FeeDestination != null ? new JRaw(lt.EngineReceipt.FeeDestination) : null,
            FeeSource = optIns.ReceiptFeeSource && lt.EngineReceipt.FeeSource != null ? new JRaw(lt.EngineReceipt.FeeSource) : null,
            CostingParameters = optIns.ReceiptCostingParameters ? new JRaw(lt.EngineReceipt.CostingParameters) : null,
            NextEpoch = lt.EngineReceipt.NextEpoch != null ? new JRaw(lt.EngineReceipt.NextEpoch) : null,
            StateUpdates = optIns.ReceiptStateChanges ? new JRaw(lt.EngineReceipt.StateUpdates) : null,
            Events = optIns.ReceiptEvents ? events?.Select(x => new GatewayModel.EventsItem(x.Name, new JRaw(x.Emitter), x.Data)).ToList() : null,
        };

        return new GatewayModel.CommittedTransactionInfo(
            stateVersion: lt.StateVersion,
            epoch: lt.Epoch,
            round: lt.RoundInEpoch,
            roundTimestamp: lt.RoundTimestamp.AsUtcIsoDateWithMillisString(),
            transactionStatus: lt.EngineReceipt.Status.ToGatewayModel(),
            affectedGlobalEntities: optIns.AffectedGlobalEntities ? lt.AffectedGlobalEntities.Select(x => entityIdToAddressMap[x]).ToList() : null,
            payloadHash: payloadHash,
            intentHash: intentHash,
            feePaid: lt.FeePaid.ToString(),
            confirmedAt: lt.RoundTimestamp,
            errorMessage: lt.EngineReceipt.ErrorMessage,
            rawHex: rawHex,
            receipt: receipt,
            message: message,
            balanceChanges: optIns.BalanceChanges ? transactionBalanceChanges : null,
            manifestInstructions: manifestInstructions,
            manifestClasses: manifestClasses
        );
    }

    public static GatewayModel.TransactionStatus ToGatewayModel(this LedgerTransactionStatus status)
    {
        return status switch
        {
            LedgerTransactionStatus.Succeeded => GatewayModel.TransactionStatus.CommittedSuccess,
            LedgerTransactionStatus.Failed => GatewayModel.TransactionStatus.CommittedFailure,
            _ => throw new UnreachableException($"Didn't expect {status} value"),
        };
    }

    public static GatewayModel.PackageVmType ToGatewayModel(this PackageVmType vmType)
    {
        return vmType switch
        {
            PackageVmType.Native => GatewayModel.PackageVmType.Native,
            PackageVmType.ScryptoV1 => GatewayModel.PackageVmType.ScryptoV1,
            _ => throw new UnreachableException($"Didn't expect {vmType} value"),
        };
    }

    public static GatewayModel.ObjectModuleId ToGatewayModel(this ModuleId moduleId)
    {
        return moduleId switch
        {
            ModuleId.Main => GatewayModel.ObjectModuleId.Main,
            ModuleId.Metadata => GatewayModel.ObjectModuleId.Metadata,
            ModuleId.Royalty => GatewayModel.ObjectModuleId.Royalty,
            ModuleId.RoleAssignment => GatewayModel.ObjectModuleId.RoleAssignment,
            _ => throw new UnreachableException($"Didn't expect {moduleId} value"),
        };
    }

    public static GatewayModel.ComponentRoyaltyConfig ToGatewayModel(this ComponentMethodRoyaltyEntryHistory[]? input)
    {
        return new GatewayModel.ComponentRoyaltyConfig(
            isEnabled: input != null,
            methodRules: input?.Select(e => new GatewayModel.ComponentMethodRoyalty(e.MethodName, TranscodeRoyaltyAmount(e.RoyaltyAmount))).ToList());
    }

    public static GatewayModel.PackageBlueprintCollectionItem ToGatewayModel(this PackageBlueprintHistory input, Dictionary<long, EntityAddress> correlatedAddresses)
    {
        return new GatewayModel.PackageBlueprintCollectionItem(
            name: input.Name,
            version: input.Version,
            definition: new JRaw(input.Definition),
            dependantEntities: input.DependantEntityIds?.Select(de => correlatedAddresses[de].ToString()).ToList(),
            authTemplate: input.AuthTemplate != null ? new JRaw(input.AuthTemplate) : null,
            authTemplateIsLocked: input.AuthTemplateIsLocked,
            royaltyConfig: input.RoyaltyConfig != null ? TranscodeBlueprintRoyaltyConfig(input.RoyaltyConfig) : null,
            royaltyConfigIsLocked: input.RoyaltyConfigIsLocked
        );
    }

    public static GatewayModel.PackageCodeCollectionItem ToGatewayModel(this PackageCodeHistory input)
    {
        return new GatewayModel.PackageCodeCollectionItem(
            vmType: input.VmType.ToGatewayModel(),
            codeHashHex: input.CodeHash.ToHex(),
            codeHex: input.Code.ToHex()
        );
    }

    public static GatewayModel.EntitySchemaCollectionItem ToGatewayModel(this SchemaEntryDefinition input)
    {
        return new GatewayModel.EntitySchemaCollectionItem(
            schemaHashHex: input.SchemaHash.ToHex(),
            schemaHex: input.Schema.ToHex()
        );
    }

    public static GatewayModel.PublicKey ToGatewayModel(this ToolkitModel.PublicKey publicKey)
    {
        return publicKey switch
        {
            ToolkitModel.PublicKey.Secp256k1 secp256k1 => new GatewayModel.PublicKeyEcdsaSecp256k1(secp256k1.value.ToArray().ToHex()),
            ToolkitModel.PublicKey.Ed25519 ed25519 => new GatewayModel.PublicKeyEddsaEd25519(ed25519.value.ToArray().ToHex()),
            _ => throw new UnreachableException($"Didn't expect {publicKey} value"),
        };
    }

    public static GatewayModel.PublicKeyHash ToGatewayModel(this ToolkitModel.PublicKeyHash publicKeyHash)
    {
        return publicKeyHash switch
        {
            ToolkitModel.PublicKeyHash.Secp256k1 secp256k1 => new GatewayModel.PublicKeyHashEcdsaSecp256k1(secp256k1.value.ToArray().ToHex()),
            ToolkitModel.PublicKeyHash.Ed25519 ed25519 => new GatewayModel.PublicKeyHashEddsaEd25519(ed25519.value.ToArray().ToHex()),
            _ => throw new UnreachableException($"Didn't expect {publicKeyHash} value"),
        };
    }

    public static GatewayModel.TransactionBalanceChanges ToGatewayModel(this CoreModel.LtsCommittedTransactionOutcome input)
    {
        var fungibleFeeBalanceChanges = new List<GatewayModel.TransactionFungibleFeeBalanceChanges>();
        var fungibleBalanceChanges = new List<GatewayModel.TransactionFungibleBalanceChanges>();

        foreach (var f in input.FungibleEntityBalanceChanges)
        {
            fungibleFeeBalanceChanges.AddRange(f.FeeBalanceChanges
                .Select(x => new GatewayModel.TransactionFungibleFeeBalanceChanges(x.Type.ToGatewayModel(), f.EntityAddress, x.ResourceAddress, x.BalanceChange)));
            fungibleBalanceChanges.AddRange(f.NonFeeBalanceChanges
                .Select(x => new GatewayModel.TransactionFungibleBalanceChanges(f.EntityAddress, x.ResourceAddress, x.BalanceChange)));
        }

        var nonFungibleBalanceChanges = input
            .NonFungibleEntityBalanceChanges
            .Select(x => new GatewayModel.TransactionNonFungibleBalanceChanges(x.EntityAddress, x.ResourceAddress, x.Added, x.Removed))
            .ToList();

        return new GatewayModel.TransactionBalanceChanges(fungibleFeeBalanceChanges, fungibleBalanceChanges, nonFungibleBalanceChanges);
    }

    public static GatewayModel.TransactionBalanceChanges ToGatewayModel(this CoreModel.CommittedTransactionBalanceChanges input)
    {
        var fungibleFeeBalanceChanges = new List<GatewayModel.TransactionFungibleFeeBalanceChanges>();
        var fungibleBalanceChanges = new List<GatewayModel.TransactionFungibleBalanceChanges>();

        foreach (var f in input.FungibleEntityBalanceChanges)
        {
            fungibleFeeBalanceChanges.AddRange(f.FeeBalanceChanges
                .Select(x => new GatewayModel.TransactionFungibleFeeBalanceChanges(x.Type.ToGatewayModel(), f.EntityAddress, x.ResourceAddress, x.BalanceChange)));
            fungibleBalanceChanges.AddRange(f.NonFeeBalanceChanges
                .Select(x => new GatewayModel.TransactionFungibleBalanceChanges(f.EntityAddress, x.ResourceAddress, x.BalanceChange)));
        }

        var nonFungibleBalanceChanges = input
            .NonFungibleEntityBalanceChanges
            .Select(x => new GatewayModel.TransactionNonFungibleBalanceChanges(x.EntityAddress, x.ResourceAddress, x.Added, x.Removed))
            .ToList();

        return new GatewayModel.TransactionBalanceChanges(fungibleFeeBalanceChanges, fungibleBalanceChanges, nonFungibleBalanceChanges);
    }

    private static GatewayModel.RoyaltyAmount? ToGatewayModel(this CoreModel.RoyaltyAmount? input)
    {
        if (input == null)
        {
            return null;
        }

        var unit = input.Unit switch
        {
            CoreModel.RoyaltyAmount.UnitEnum.XRD => GatewayModel.RoyaltyAmount.UnitEnum.XRD,
            CoreModel.RoyaltyAmount.UnitEnum.USD => GatewayModel.RoyaltyAmount.UnitEnum.USD,
            _ => throw new UnreachableException($"Didn't expect {input.Unit} value"),
        };

        return new GatewayModel.RoyaltyAmount(input.Amount, unit);
    }

    private static GatewayModel.TransactionFungibleFeeBalanceChangeType ToGatewayModel(this CoreModel.LtsFeeFungibleResourceBalanceChangeType input)
    {
        return input switch
        {
            CoreModel.LtsFeeFungibleResourceBalanceChangeType.FeePayment => GatewayModel.TransactionFungibleFeeBalanceChangeType.FeePayment,
            CoreModel.LtsFeeFungibleResourceBalanceChangeType.FeeDistributed => GatewayModel.TransactionFungibleFeeBalanceChangeType.FeeDistributed,
            CoreModel.LtsFeeFungibleResourceBalanceChangeType.TipDistributed => GatewayModel.TransactionFungibleFeeBalanceChangeType.TipDistributed,
            CoreModel.LtsFeeFungibleResourceBalanceChangeType.RoyaltyDistributed => GatewayModel.TransactionFungibleFeeBalanceChangeType.RoyaltyDistributed,
            _ => throw new UnreachableException($"Didn't expect {input} value"),
        };
    }

    private static GatewayModel.RoyaltyAmount? TranscodeRoyaltyAmount(string? input)
    {
        if (input == null)
        {
            return null;
        }

        return JsonConvert.DeserializeObject<CoreModel.RoyaltyAmount>(input).ToGatewayModel();
    }

    private static GatewayModel.BlueprintRoyaltyConfig? TranscodeBlueprintRoyaltyConfig(string? input)
    {
        if (input == null)
        {
            return null;
        }

        var coreModel = JsonConvert.DeserializeObject<CoreModel.BlueprintRoyaltyConfig>(input);

        if (coreModel == null)
        {
            return null;
        }

        List<GatewayModel.BlueprintMethodRoyalty>? methodRules = null;

        if (coreModel.MethodRules != null)
        {
            methodRules = coreModel.MethodRules
                .Select(mr => new GatewayModel.BlueprintMethodRoyalty(mr.MethodName, mr.RoyaltyAmount.ToGatewayModel()))
                .ToList();
        }

        return new GatewayModel.BlueprintRoyaltyConfig(coreModel.IsEnabled, methodRules);
    }
}
