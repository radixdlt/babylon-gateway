// using System.Threading.Tasks;
// using CoreModel = RadixDlt.CoreApiSdk.Model;
//
// namespace RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension;
//
// internal class NonFungibleVaultProcessor
// {
//     private readonly ProcessorContext _context;
//     private readonly ReferencedEntityDictionary _referencedEntities;
//
//     public NonFungibleVaultProcessor(ProcessorContext context, ReferencedEntityDictionary referencedEntities)
//     {
//         _context = context;
//         _referencedEntities = referencedEntities;
//     }
//
//     public void VisitUpsert(CoreModel.Substate substateData, ReferencedEntity referencedEntity, long stateVersion)
//     {
//
//     }
//
//     public async Task LoadDependencies()
//     {
//         _existingFungibleDefinitions.AddRange(await ExistingComponentFungibleResourceDefinitions());
//         _existingNonFungibleDefinitions.AddRange(await ExistingComponentNonFungibleResourceDefinitions());
//         _mostRecentFungibleTotalsHistory.AddRange(await MostRecentComponentFungibleResourceTotalsHistory());
//         _mostRecentNonFungibleTotalsHistory.AddRange(await MostRecentComponentNonFungibleResourceTotalsHistory());
//     }
//
//     public void ProcessChanges()
//     {
//
//     }
//
//     public async Task<int> SaveEntities()
//     {
//         var rowsInserted = 0;
//
//         rowsInserted += await CopyComponentFungibleResourceDefinitions();
//         rowsInserted += await CopyComponentNonFungibleResourceDefinitions();
//         rowsInserted += await CopyComponentFungibleResourceTotalsHistory();
//         rowsInserted += await CopyComponentNonFungibleResourceTotalsHistory();
//
//         return rowsInserted;
//     }
// }
