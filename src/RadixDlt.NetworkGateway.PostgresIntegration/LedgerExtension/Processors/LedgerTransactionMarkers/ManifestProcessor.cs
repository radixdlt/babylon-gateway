using RadixDlt.NetworkGateway.Abstractions.Extensions;
using RadixDlt.NetworkGateway.Abstractions.Model;
using RadixDlt.NetworkGateway.Abstractions.Network;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using RadixDlt.NetworkGateway.PostgresIntegration.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using CoreModel = RadixDlt.CoreApiSdk.Model;
using ToolkitModel = RadixEngineToolkit;

namespace RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension.Processors.LedgerTransactionMarkers;

internal class ManifestProcessor : ITransactionMarkerProcessor
{
    private readonly ProcessorContext _context;
    private readonly ReferencedEntityDictionary _referencedEntities;
    private readonly NetworkConfiguration _networkConfiguration;
    private readonly List<LedgerTransactionMarker> _ledgerTransactionMarkersToAdd = new();

    private readonly Dictionary<long, ManifestAddressesExtractor.ManifestAddresses> _manifestExtractedAddresses = new();
    private readonly Dictionary<long, List<LedgerTransactionManifestClass>> _manifestClasses = new();

    public ManifestProcessor(ProcessorContext context, ReferencedEntityDictionary referencedEntities, NetworkConfiguration networkConfiguration)
    {
        _context = context;
        _referencedEntities = referencedEntities;
        _networkConfiguration = networkConfiguration;
    }

    public void VisitTransaction(CoreModel.CommittedTransaction transaction, long stateVersion)
    {
        if (transaction.LedgerTransaction is CoreModel.UserLedgerTransaction userLedgerTransaction)
        {
            var coreInstructions = userLedgerTransaction.NotarizedTransaction.SignedIntent.Intent.Instructions;
            var coreBlobs = userLedgerTransaction.NotarizedTransaction.SignedIntent.Intent.BlobsHex;
            using var manifestInstructions = ToolkitModel.Instructions.FromString(coreInstructions, _networkConfiguration.Id);
            using var toolkitManifest = new ToolkitModel.TransactionManifest(manifestInstructions, coreBlobs.Values.Select(x => x.ConvertFromHex()).ToArray());

            var extractedAddresses = ManifestAddressesExtractor.ExtractAddresses(toolkitManifest, _networkConfiguration.Id);

            foreach (var address in extractedAddresses.All())
            {
                _referencedEntities.MarkSeenAddress(address);
            }

            _manifestExtractedAddresses.Add(stateVersion, extractedAddresses);

            AnalyzeManifestClasses(toolkitManifest, stateVersion);
        }
    }

    public IEnumerable<LedgerTransactionMarker> CreateTransactionMarkers()
    {
        foreach (var stateVersion in _manifestExtractedAddresses.Keys)
        {
            AnalyzeAddresses(stateVersion);
        }

        return _ledgerTransactionMarkersToAdd;
    }

    public LedgerTransactionManifestClass[] GetManifestClasses(long stateVersion)
    {
        return _manifestClasses.TryGetValue(stateVersion, out var mc) ? mc.ToArray() : Array.Empty<LedgerTransactionManifestClass>();
    }

    public void AnalyzeManifestClasses(ToolkitModel.TransactionManifest toolkitManifest, long stateVersion)
    {
        var manifestSummary = toolkitManifest.Summary(_networkConfiguration.Id);

        for (var i = 0; i < manifestSummary.classification.Length; ++i)
        {
            var manifestClass = manifestSummary.classification[i].ToModel();

            _manifestClasses
                .GetOrAdd(stateVersion, _ => new List<LedgerTransactionManifestClass>())
                .Add(manifestClass);

            _ledgerTransactionMarkersToAdd.Add(
                new ManifestClassMarker
                {
                    Id = _context.Sequences.LedgerTransactionMarkerSequence++,
                    StateVersion = stateVersion,
                    ManifestClass = manifestClass,
                    IsMostSpecific = i == 0,
                });
        }
    }

    private void AnalyzeAddresses(long stateVersion)
    {
        if (_manifestExtractedAddresses.TryGetValue(stateVersion, out var extractedAddresses))
        {
            foreach (var proofResourceAddress in extractedAddresses.PresentedProofs.Select(x => x.ResourceAddress).ToHashSet())
            {
                if (_referencedEntities.TryGet(proofResourceAddress, out var re))
                {
                    _ledgerTransactionMarkersToAdd.Add(
                        new ManifestAddressLedgerTransactionMarker
                        {
                            Id = _context.Sequences.LedgerTransactionMarkerSequence++,
                            StateVersion = stateVersion,
                            OperationType = LedgerTransactionMarkerOperationType.BadgePresented,
                            EntityId = re.DatabaseId,
                        });
                }
            }

            foreach (var address in extractedAddresses.ResourceAddresses)
            {
                if (_referencedEntities.TryGet(address, out var re))
                {
                    _ledgerTransactionMarkersToAdd.Add(
                        new ManifestAddressLedgerTransactionMarker
                        {
                            Id = _context.Sequences.LedgerTransactionMarkerSequence++,
                            StateVersion = stateVersion,
                            OperationType = LedgerTransactionMarkerOperationType.ResourceInUse,
                            EntityId = re.DatabaseId,
                        });
                }
            }

            foreach (var address in extractedAddresses.AccountsRequiringAuth)
            {
                if (_referencedEntities.TryGet(address, out var re))
                {
                    _ledgerTransactionMarkersToAdd.Add(
                        new ManifestAddressLedgerTransactionMarker
                        {
                            Id = _context.Sequences.LedgerTransactionMarkerSequence++,
                            StateVersion = stateVersion,
                            OperationType = LedgerTransactionMarkerOperationType.AccountOwnerMethodCall,
                            EntityId = re.DatabaseId,
                        });
                }
            }

            foreach (var address in extractedAddresses.AccountsDepositedInto)
            {
                if (_referencedEntities.TryGet(address, out var re))
                {
                    _ledgerTransactionMarkersToAdd.Add(
                        new ManifestAddressLedgerTransactionMarker
                        {
                            Id = _context.Sequences.LedgerTransactionMarkerSequence++,
                            StateVersion = stateVersion,
                            OperationType = LedgerTransactionMarkerOperationType.AccountDepositedInto,
                            EntityId = re.DatabaseId,
                        });
                }
            }

            foreach (var address in extractedAddresses.AccountsWithdrawnFrom)
            {
                if (_referencedEntities.TryGet(address, out var re))
                {
                    _ledgerTransactionMarkersToAdd.Add(
                        new ManifestAddressLedgerTransactionMarker
                        {
                            Id = _context.Sequences.LedgerTransactionMarkerSequence++,
                            StateVersion = stateVersion,
                            OperationType = LedgerTransactionMarkerOperationType.AccountWithdrawnFrom,
                            EntityId = re.DatabaseId,
                        });
                }
            }
        }
    }
}
