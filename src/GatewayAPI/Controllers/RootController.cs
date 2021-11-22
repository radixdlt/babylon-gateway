using Microsoft.AspNetCore.Mvc;

namespace GatewayAPI.Controllers;

[ApiController]
[Route("")]
public class RootController : ControllerBase
{
    [HttpGet("")]
    public object GetRootResponse()
    {
        return new
        {
            docs = "https://docs.radixdlt.com",
            repo = "https://github.com/radixdlt/radixdlt-network-gateway",
        };
    }
}
