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

    [HttpPost("do-x")]
    public async Task<GatewayResponse> DoX([FromBody] MyReq req, [FromQuery] bool? t = false)
    {
        if (t == true)
        {
            throw new Exception("xxx");
        }

        return await _ledgerStateQuerier.GetGatewayState();
    }
}

public class MyReq
{
    [Required]
    public string SomeString { get; set; }
}
