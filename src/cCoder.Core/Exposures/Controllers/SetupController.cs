// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

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
        if (await setupStateService.IsInitializedAsync(cancellationToken: cancellationToken))
        {
            return Redirect(url: "/");
        }

        return View(model: CreateViewModel());
    }

    [HttpPost("")]
    public async Task<IActionResult> Index(
        [Bind(Prefix = "Setup")] FirstTimeSetupRequest setup,
        CancellationToken cancellationToken)
    {
        try
        {
            if (await setupStateService.IsInitializedAsync(cancellationToken: cancellationToken))
            {
                return Redirect(url: "/");
            }

            if (!ModelState.IsValid)
            {
                return View(model: CreateViewModel(setup: setup));
            }

            setup.Domain = SetupRequestHostNormalizer.Normalize(host: Request.Host.Host);

            FirstTimeSetupResult result = await setupOrchestrationService.SetupAsync(
request: setup, cancellationToken: cancellationToken);

            await userRegistrationOrchestrationService.LoginAsync(username: result.UserId, password: setup.Password);

            return Redirect(url: "/");
        }
        catch (Exception ex)
        {
            log.LogError(exception: ex, message: "First-time setup failed.");
            ModelState.AddModelError(key: string.Empty, errorMessage: ex.Message);
            return View(model: CreateViewModel(setup: setup));
        }
    }

    private FirstTimeSetupViewModel CreateViewModel(FirstTimeSetupRequest setup = null) =>
        new()
        {
            Domain = SetupRequestHostNormalizer.Normalize(host: Request.Host.Host),
            Setup = setup ?? new FirstTimeSetupRequest(),
        };
}