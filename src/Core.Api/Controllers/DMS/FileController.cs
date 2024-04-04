using cCoder.Core.Objects;
using cCoder.Core.Services;
using File = cCoder.Core.Objects.Entities.DMS.File;

namespace cCoder.Core.Api.Controllers
{
    public class FileController : CoreEntityODataController<File, Guid>
    {
        protected new IFileService Service => base.Service as IFileService;

        public FileController(IFileService service, ICoreAuthInfo auth, ILogger<FileController> log) 
            : base(service, auth, log) { }
    }
}