using cCoder.Core.Services.Orchestrations;
using cCoder.Mail.Models;
using cCoder.Data.Models.CMS;
using cCoder.Data.Models.Mail;
using cCoder.Data.Models.Security;
using Microsoft.AspNetCore.Mvc;

namespace cCoder.Core.Exposures.Controllers;

[ApiController]
public class TemplatedEmailController(
    ITemplatedEmailOrchestrationService templatedEmailOrchestrationService) : ControllerBase
{
    [HttpPost("Api/Core/QueuedEmail/AddTemplatedEmail()")]
    public async Task<IActionResult> AddTemplatedEmail([FromBody] TemplatedEmailDetails details)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        return Ok(await templatedEmailOrchestrationService.QueueAsync(details));
    }
}

