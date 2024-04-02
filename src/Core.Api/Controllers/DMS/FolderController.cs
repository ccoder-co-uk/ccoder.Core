using Core.Objects;
using Core.Objects.Entities.DMS;
using Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace Core.Api.Controllers
{
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
}