using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.Packaging;
using cCoder.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace cCoder.Core.Api.Controllers.CMS;

public class PackageController(IPackageService service, ICoreAuthInfo auth, ILogger<PackageController> log)
    : CoreEntityODataController<Package, Guid>(service, auth, log)
{
    private readonly ILogger log = log;

    protected new IPackageService Service =>
        base.Service as IPackageService;

    [HttpPost]
    public async Task<IActionResult> Import(int appId, string packageUrl)
    {
        StringValues remoteAuth = Request.Headers["remote-auth"];
        await Service.Import(appId, packageUrl, remoteAuth);
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> ImportThis(int appId)
    {
        try
        {
            using StreamReader reader = new(Request.Body);
            string data = await reader.ReadToEndAsync();

            if (data.StartsWith('{'))
            {
                Package package = Objects.Data.ParseJson<Package>(data);
                await Service.Import(appId, package);
            }
            else
            {
                Package[] packages = Objects.Data.ParseJson<Package[]>(data);

                foreach (Package package in packages)
                    await Service.Import(appId, package);
            }
        }
        catch (Exception ex)
        {
            log.LogWarning("A Packkage import failed due to an exception:\n", ex);
            return BadRequest(ex);
        }
        return Ok();
    }
}