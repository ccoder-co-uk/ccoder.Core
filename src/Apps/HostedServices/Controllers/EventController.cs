using cCoder.Eventing.Http.Models;
using Microsoft.AspNetCore.Mvc;

namespace HostedServices.Controllers;

[ApiController]
[Route("Api/Eventing")]
public sealed class EventController(ReceivedHttpEventProcessor processor) : ControllerBase
{
    [HttpPost]
    public async ValueTask<IActionResult> Post(
        HttpEventMessage message,
        CancellationToken cancellationToken)
    {
        await processor.ProcessAsync(message, cancellationToken);
        return Ok();
    }
}