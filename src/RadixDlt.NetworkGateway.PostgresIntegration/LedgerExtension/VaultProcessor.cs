using RadixDlt.NetworkGateway.Abstractions.Numerics;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using CoreModel = RadixDlt.CoreApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension;

internal class VaultProcessor
{
    private readonly ProcessorContext _context;
    private readonly ReferencedEntityDictionary _referencedEntities;

    public VaultProcessor(ProcessorContext context, ReferencedEntityDictionary referencedEntities)
    {
        _context = context;
        _referencedEntities = referencedEntities;
    }

    public void VisitUpsert(CoreModel.Substate substateData, ReferencedEntity referencedEntity, long stateVersion)
    {
        if (substateData is CoreModel.FungibleVaultFieldBalanceSubstate fungibleVaultFieldBalanceSubstate)
        {
            var vaultEntity = referencedEntity.GetDatabaseEntity<InternalFungibleVaultEntity>();
            var resourceEntity = _referencedEntities.GetByDatabaseId(vaultEntity.GetResourceEntityId());
            var amount = TokenAmount.FromDecimalString(fungibleVaultFieldBalanceSubstate.Value.Amount);

            // vaultSnapshots.Add(new FungibleVaultSnapshot(referencedEntity, resourceEntity, amount, stateVersion));
        }

        if (substateData is CoreModel.NonFungibleVaultContentsIndexEntrySubstate nonFungibleVaultContentsIndexEntrySubstate)
        {
            var vaultEntity = referencedEntity.GetDatabaseEntity<InternalNonFungibleVaultEntity>();
            var resourceEntity = _referencedEntities.GetByDatabaseId(vaultEntity.GetResourceEntityId());
            var simpleRep = nonFungibleVaultContentsIndexEntrySubstate.Key.NonFungibleLocalId.SimpleRep;

            // vaultSnapshots.Add(new NonFungibleVaultSnapshot(referencedEntity, resourceEntity, simpleRep, false, stateVersion));
        }
    }

    public void VisitDelete(CoreModel.SubstateId substateId, ReferencedEntity referencedEntity, long stateVersion)
    {
        if (substateId.SubstateType == CoreModel.SubstateType.NonFungibleVaultContentsIndexEntry)
        {
            var resourceEntity = _referencedEntities.GetByDatabaseId(referencedEntity.GetDatabaseEntity<InternalNonFungibleVaultEntity>().GetResourceEntityId());
            var simpleRep = ScryptoSborUtils.GetNonFungibleId(((CoreModel.MapSubstateKey)substateId.SubstateKey).KeyHex);

            // vaultSnapshots.Add(new NonFungibleVaultSnapshot(referencedEntity, resourceEntity, simpleRep, true, stateVersion));
        }
    }
}
