using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using CoreModel = RadixDlt.CoreApiSdk.Model;
using ToolkitModel = RadixEngineToolkit;

namespace RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension;

internal interface IProcessorBase
{
    Task LoadDependenciesAsync();

    void ProcessChanges();

    Task<int> SaveEntitiesAsync();
}

internal interface IDecodedEventProcessor
{
    void VisitDecodedEvent(ToolkitModel.TypedNativeEvent decodedEvent, ReferencedEntity eventEmitterEntity, long stateVersion);
}

internal interface ITransactionProcessor
{
    void VisitTransaction(CoreModel.CommittedTransaction transaction, long stateVersion);
}

internal interface IEventProcessor
{
    void VisitEvent(CoreModel.Event @event, long stateVersion);
}

internal interface ISubstateUpsertProcessor
{
    void VisitUpsert(CoreModel.IUpsertedSubstate substate, ReferencedEntity referencedEntity, long stateVersion);
}

internal interface ISubstateDeleteProcessor
{
    void VisitDelete(CoreModel.SubstateId substateId, ReferencedEntity referencedEntity, long stateVersion);
}

internal interface ITransactionMarkerProcessor
{
    IEnumerable<LedgerTransactionMarker> CreateTransactionMarkers();
}
