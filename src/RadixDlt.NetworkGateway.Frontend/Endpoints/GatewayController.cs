using Microsoft.AspNetCore.Mvc;
using RadixDlt.NetworkGateway.Frontend.Services;
using RadixDlt.NetworkGateway.FrontendSdk.Model;
using System.ComponentModel.DataAnnotations;

namespace RadixDlt.NetworkGateway.Frontend.Endpoints;

[ApiController]
[Route("gateway")]
[TypeFilter(typeof(ExceptionFilter))]
[TypeFilter(typeof(InvalidModelStateFilter))]
public class GatewayController : ControllerBase
{
    private readonly ILedgerStateQuerier _ledgerStateQuerier;

    public GatewayController(ILedgerStateQuerier ledgerStateQuerier)
    {
        _ledgerStateQuerier = ledgerStateQuerier;
    }

    [HttpPost("")]
    public async Task<GatewayResponse> Status()
    {
        return await _ledgerStateQuerier.GetGatewayState();
    }
}
