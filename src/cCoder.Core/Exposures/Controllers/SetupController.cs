// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;
using cCoder.Core.Models;
using cCoder.Core.Services.Setup;
using cCoder.Core.Exposures.Setup;

namespace cCoder.Core.Exposures.Controllers;

[Route("Setup")]
public sealed class SetupController(
    IFirstTimeSetupStateService setupStateService,
    ISetupRequestHostManager setupRequestHostManager,
    CoreConfiguration configuration)
    : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        if (await setupStateService.IsInitializedAsync(
            cancellationToken: cancellationToken))
        {
            return Redirect(url: "/");
        }

        return View(
            viewName: "Index",
            model: CreateFirstTimeSetupViewModel());
    }

    private FirstTimeSetupViewModel CreateFirstTimeSetupViewModel() =>
        new()
        {
            AssetsRoot = configuration.Packaging.AssetsRoot,
            Domain = setupRequestHostManager.NormalizeHost(
                host: Request.Host.Host),
            Setup = new FirstTimeSetupRequest(),
        };
}