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
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        if (await setupStateService.IsInitializedAsync(cancellationToken: cancellationToken))
        {
            return Redirect(url: "/");
        }

        return View(
            viewName: "Index",
            model: CreateFirstTimeSetupViewModel());
    }

    [HttpPost("")]
    public async Task<IActionResult> Post(
        [Bind(Prefix = "Setup")] FirstTimeSetupRequest newFirstTimeSetupRequest,
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
                return View(
                    viewName: "Index",
                    model: CreateFirstTimeSetupViewModel(
                        setup: newFirstTimeSetupRequest));
            }

            newFirstTimeSetupRequest.Domain =
                SetupRequestHostNormalizer.Normalize(
                    host: Request.Host.Host);

            FirstTimeSetupResult result = await setupOrchestrationService.SetupAsync(
                request: newFirstTimeSetupRequest,
                cancellationToken: cancellationToken);

            await userRegistrationOrchestrationService.LoginAsync(
                username: result.UserId,
                password: newFirstTimeSetupRequest.Password);

            return Redirect(url: "/");
        }
        catch (Exception ex)
        {
            log.LogError(exception: ex, message: "First-time setup failed.");
            ModelState.AddModelError(key: string.Empty, errorMessage: ex.Message);

            return View(
                viewName: "Index",
                model: CreateFirstTimeSetupViewModel(
                    setup: newFirstTimeSetupRequest));
        }
    }

    private FirstTimeSetupViewModel CreateFirstTimeSetupViewModel(
        FirstTimeSetupRequest setup = null) =>
        new()
        {
            Domain = SetupRequestHostNormalizer.Normalize(host: Request.Host.Host),
            Setup = setup ?? new FirstTimeSetupRequest(),
        };
}