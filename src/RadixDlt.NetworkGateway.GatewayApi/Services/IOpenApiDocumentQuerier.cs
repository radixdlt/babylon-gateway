using System.Threading;
using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.GatewayApi.Services;

public record OpenApiDocumentPlaceholderData(string? RandomIntentHash, string? RandomSubintentHash, long? CurrentEpoch, string? RequirementResourceAddress, string? RequirementNonFungibleId);

public interface IOpenApiDocumentQuerier
{
    Task<OpenApiDocumentPlaceholderData> GetPlaceholderData(CancellationToken token = default);
}
