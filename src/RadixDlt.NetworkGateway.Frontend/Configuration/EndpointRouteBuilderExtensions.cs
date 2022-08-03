using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using RadixDlt.NetworkGateway.Frontend.Endpoints;
using RadixDlt.NetworkGateway.FrontendSdk.Model;

namespace RadixDlt.NetworkGateway.Frontend.Configuration;

public static class EndpointRouteBuilderExtensions
{
    public static void MapNetworkGatewayApi(this IEndpointRouteBuilder endpoints)
    {
        // TODO make endpoints configurable?
        // TODO or maybe use fully-fledged MVC controllers and actions due to their superior capabilities?

        endpoints.MapPost("/transaction/recent", (TransactionEndpoint te, [FromBody] RecentTransactionsRequest request, CancellationToken token) => te.Recent(request, token));
        endpoints.MapPost("/test4", (TransactionEndpoint te, [FromBody] TransactionBuildRequest request, CancellationToken token) => te.Build(request, token));
        endpoints.MapPost("/gateway", (GatewayEndpoint ge, [FromQuery] int? myInt, CancellationToken token) => ge.Status(token));
    }
}
