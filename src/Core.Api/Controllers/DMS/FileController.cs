using Core.Objects;
using Core.Services;
using File = Core.Objects.Entities.DMS.File;

namespace Core.Api.Controllers
{
    public class FileController : CoreEntityODataController<File, Guid>
    {
        protected new IFileService Service => base.Service as IFileService;

        public FileController(IFileService service, ICoreAuthInfo auth, ILogger<FileController> log) 
            : base(service, auth, log) { }
    }
}