using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.DMS;
using cCoder.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace cCoder.Core.Api.Controllers.DMS;

public class FolderController : CoreEntityODataController<Folder, Guid>
{

    protected new IFolderService Service =>
        base.Service as IFolderService;

    public FolderController(IFolderService service, ICoreAuthInfo auth, ILogger<FolderController> log)
        : base(service, auth, log) { }

    [HttpPost]
    public async Task<IActionResult> Copy(string source, string destination, int sourceAppId, int destAppId) =>
        Ok(await Service.Copy(source, destination, sourceAppId, destAppId));
}