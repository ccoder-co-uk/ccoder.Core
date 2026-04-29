using cCoder.Eventing.Http;
using cCoder.Eventing.Http.Models;
using Microsoft.AspNetCore.Mvc;

namespace HostedServices.Controllers;

[ApiController]
[Route("Api/Eventing")]
public sealed class EventController(IHttpEventHub httpEventHub) : ControllerBase
{
    [HttpPost]
    public async ValueTask<IActionResult> Post(
        HttpEventMessage message,
        CancellationToken cancellationToken)
    {
        await httpEventHub.ReceiveEventAsync(message, cancellationToken);
        return Accepted();
    }
}
