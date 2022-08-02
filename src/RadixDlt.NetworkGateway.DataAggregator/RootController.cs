using Microsoft.AspNetCore.Mvc;

namespace RadixDlt.NetworkGateway.DataAggregator;

[ApiController]
public class RootController : ControllerBase
{
    [HttpGet("")]
    public IActionResult GetRootResponse()
    {
        return Ok(new
        {
            docs = "https://docs.radixdlt.com",
            repo = "https://github.com/radixdlt/radixdlt-network-gateway",
            version = "???",
            ledger_commit_health = "???",
        });
    }
}
