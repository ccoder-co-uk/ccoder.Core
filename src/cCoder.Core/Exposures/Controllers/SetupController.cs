using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using cCoder.Core.Models;
using cCoder.Core.Services.Setup;

namespace cCoder.Core.Exposures.Controllers;

[Route("Setup")]
public sealed class SetupController(
    IFirstTimeSetupStateService setupStateService,
    IFirstTimeSetupOrchestrationService setupOrchestrationService,
    cCoder.Core.Services.Orchestrations.IUserRegistrationOrchestrationService userRegistrationOrchestrationService,
    ILogger<SetupController> log)
    : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        if (await setupStateService.IsInitializedAsync(cancellationToken))
            return Redirect("/");

        return View(CreateViewModel());
    }

    [HttpPost("")]
    public async Task<IActionResult> Index(
        [Bind(Prefix = "Setup")] FirstTimeSetupRequest setup,
        CancellationToken cancellationToken)
    {
        try
        {
            if (await setupStateService.IsInitializedAsync(cancellationToken))
                return Redirect("/");

            if (!ModelState.IsValid)
                return View(CreateViewModel(setup));

            setup.Domain = SetupRequestHostNormalizer.Normalize(Request.Host.Host);

            FirstTimeSetupResult result = await setupOrchestrationService.SetupAsync(
                setup,
                cancellationToken);

            await userRegistrationOrchestrationService.LoginAsync(result.UserId, setup.Password);

            return Redirect("/");
        }
        catch (Exception ex)
        {
            log.LogError(ex, "First-time setup failed.");
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(CreateViewModel(setup));
        }
    }

    private FirstTimeSetupViewModel CreateViewModel(FirstTimeSetupRequest setup = null) =>
        new()
        {
            Domain = SetupRequestHostNormalizer.Normalize(Request.Host.Host),
            Setup = setup ?? new FirstTimeSetupRequest(),
        };
}
