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

using RadixDlt.NetworkGateway.Abstractions.Extensions;
using RadixDlt.NetworkGateway.Abstractions.Model;
using RadixDlt.NetworkGateway.Abstractions.Network;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using RadixDlt.NetworkGateway.PostgresIntegration.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CoreModel = RadixDlt.CoreApiSdk.Model;
using ToolkitModel = RadixEngineToolkit;

namespace RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension.Processors.LedgerTransactionMarkers;

internal class ManifestProcessor : ITransactionMarkerProcessor, ITransactionScanProcessor
{
    private readonly ProcessorContext _context;
    private readonly ReferencedEntityDictionary _referencedEntities;
    private readonly NetworkConfiguration _networkConfiguration;

    private readonly Dictionary<long, ManifestAddressesExtractor.ManifestAddresses> _manifestExtractedAddresses = new();
    private readonly Dictionary<long, List<LedgerTransactionManifestClass>> _manifestClasses = new();

    public ManifestProcessor(ProcessorContext context, ReferencedEntityDictionary referencedEntities, NetworkConfiguration networkConfiguration)
    {
        _context = context;
        _referencedEntities = referencedEntities;
        _networkConfiguration = networkConfiguration;
    }

    public void OnTransactionScan(CoreModel.CommittedTransaction transaction, long stateVersion)
    {
        if (transaction.LedgerTransaction is CoreModel.UserLedgerTransaction userLedgerTransaction)
        {
            var coreInstructions = userLedgerTransaction.NotarizedTransaction.SignedIntent.Intent.Instructions;
            var coreBlobs = userLedgerTransaction.NotarizedTransaction.SignedIntent.Intent.BlobsHex;
            using var manifestInstructions = ToolkitModel.Instructions.FromString(coreInstructions, _networkConfiguration.Id);
            using var toolkitManifest = new ToolkitModel.TransactionManifest(manifestInstructions, coreBlobs.Values.Select(x => x.ConvertFromHex()).ToArray());

            AnalyzeManifestClasses(toolkitManifest, stateVersion);

            if (transaction.Receipt.Status == CoreModel.TransactionStatus.Succeeded)
            {
                var extractedAddresses = ManifestAddressesExtractor.ExtractAddresses(toolkitManifest, _networkConfiguration.Id);

                foreach (var address in extractedAddresses.All())
                {
                    _referencedEntities.MarkSeenAddress(address);
                }

                _manifestExtractedAddresses.Add(stateVersion, extractedAddresses);
            }
        }
    }

    public IEnumerable<LedgerTransactionMarker> CreateTransactionMarkers()
    {
        var ledgerTransactionMarkersToAdd = new List<LedgerTransactionMarker>();

        ledgerTransactionMarkersToAdd.AddRange(CreateMarkersForManifestAddresses());
        ledgerTransactionMarkersToAdd.AddRange(CreateMarkersForManifestClasses());

        return ledgerTransactionMarkersToAdd;
    }

    public LedgerTransactionManifestClass[] GetManifestClasses(long stateVersion)
    {
        return _manifestClasses.TryGetValue(stateVersion, out var mc) ? mc.ToArray() : Array.Empty<LedgerTransactionManifestClass>();
    }

    private void AnalyzeManifestClasses(ToolkitModel.TransactionManifest toolkitManifest, long stateVersion)
    {
        var manifestSummary = toolkitManifest.Summary(_networkConfiguration.Id);

        foreach (var manifestClass in manifestSummary.classification)
        {
            var mapped = manifestClass.ToModel();

            _manifestClasses
                .GetOrAdd(stateVersion, _ => new List<LedgerTransactionManifestClass>())
                .Add(mapped);
        }
    }

    private IEnumerable<LedgerTransactionMarker> CreateMarkersForManifestClasses()
    {
        var ledgerTransactionMarkersToAdd = new List<LedgerTransactionMarker>();

        foreach (var stateVersion in _manifestClasses.Keys)
        {
            for (int i = 0; i < _manifestClasses[stateVersion].Count; ++i)
            {
                ledgerTransactionMarkersToAdd.Add(
                    new ManifestClassMarker
                    {
                        Id = _context.Sequences.LedgerTransactionMarkerSequence++,
                        StateVersion = stateVersion,
                        ManifestClass = _manifestClasses[stateVersion][i],
                        IsMostSpecific = i == 0,
                    });
            }
        }

        return ledgerTransactionMarkersToAdd;
    }

    private IEnumerable<LedgerTransactionMarker> CreateMarkersForManifestAddresses()
    {
        var ledgerTransactionMarkersToAdd = new List<LedgerTransactionMarker>();

        foreach (var stateVersion in _manifestExtractedAddresses.Keys)
        {
            if (!_manifestExtractedAddresses.TryGetValue(stateVersion, out var extractedAddresses))
            {
                return ledgerTransactionMarkersToAdd;
            }

            foreach (var proofResourceAddress in extractedAddresses.PresentedProofs.Select(x => x.ResourceAddress).ToHashSet())
            {
                if (!_referencedEntities.TryGet(proofResourceAddress, out var referencedEntity))
                {
                    throw new UnreachableException($"Entity: {proofResourceAddress} was not present in referenced entities dictionary.");
                }

                ledgerTransactionMarkersToAdd.Add(
                    new ManifestAddressLedgerTransactionMarker
                    {
                        Id = _context.Sequences.LedgerTransactionMarkerSequence++,
                        StateVersion = stateVersion,
                        OperationType = LedgerTransactionMarkerOperationType.BadgePresented,
                        EntityId = referencedEntity.DatabaseId,
                    });
            }

            foreach (var resourceAddress in extractedAddresses.ResourceAddresses)
            {
                if (!_referencedEntities.TryGet(resourceAddress, out var referencedEntity))
                {
                    throw new UnreachableException($"Entity: {resourceAddress} was not present in referenced entities dictionary.");
                }

                ledgerTransactionMarkersToAdd.Add(
                    new ManifestAddressLedgerTransactionMarker
                    {
                        Id = _context.Sequences.LedgerTransactionMarkerSequence++,
                        StateVersion = stateVersion,
                        OperationType = LedgerTransactionMarkerOperationType.ResourceInUse,
                        EntityId = referencedEntity.DatabaseId,
                    });
            }

            foreach (var entityAddress in extractedAddresses.AccountsRequiringAuth)
            {
                if (_referencedEntities.TryGet(entityAddress, out var referencedEntity))
                {
                    ledgerTransactionMarkersToAdd.Add(
                        new ManifestAddressLedgerTransactionMarker
                        {
                            Id = _context.Sequences.LedgerTransactionMarkerSequence++,
                            StateVersion = stateVersion,
                            OperationType = LedgerTransactionMarkerOperationType.AccountOwnerMethodCall,
                            EntityId = referencedEntity.DatabaseId,
                        });
                }
                else if (!entityAddress.Decode().IsPreAllocatedAccountAddress())
                {
                    throw new UnreachableException($"Entity: {entityAddress} was not present in referenced entities dictionary.");
                }
            }

            foreach (var entityAddress in extractedAddresses.AccountsDepositedInto)
            {
                if (_referencedEntities.TryGet(entityAddress, out var referencedEntity))
                {
                    ledgerTransactionMarkersToAdd.Add(
                        new ManifestAddressLedgerTransactionMarker
                        {
                            Id = _context.Sequences.LedgerTransactionMarkerSequence++,
                            StateVersion = stateVersion,
                            OperationType = LedgerTransactionMarkerOperationType.AccountDepositedInto,
                            EntityId = referencedEntity.DatabaseId,
                        });
                }
                else if (!entityAddress.Decode().IsPreAllocatedAccountAddress())
                {
                    throw new UnreachableException($"Entity: {entityAddress} was not present in referenced entities dictionary.");
                }
            }

            foreach (var entityAddress in extractedAddresses.AccountsWithdrawnFrom)
            {
                if (_referencedEntities.TryGet(entityAddress, out var referencedEntity))
                {
                    ledgerTransactionMarkersToAdd.Add(
                        new ManifestAddressLedgerTransactionMarker
                        {
                            Id = _context.Sequences.LedgerTransactionMarkerSequence++,
                            StateVersion = stateVersion,
                            OperationType = LedgerTransactionMarkerOperationType.AccountWithdrawnFrom,
                            EntityId = referencedEntity.DatabaseId,
                        });
                }
                else if (!entityAddress.Decode().IsPreAllocatedAccountAddress())
                {
                    throw new UnreachableException($"Entity: {entityAddress} was not present in referenced entities dictionary.");
                }
            }
        }

        return ledgerTransactionMarkersToAdd;
    }
}
