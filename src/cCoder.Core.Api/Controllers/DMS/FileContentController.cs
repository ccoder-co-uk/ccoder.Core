using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.DMS;
using cCoder.Core.Services;

namespace cCoder.Core.Api.Controllers.DMS
{
    public class FileContentController : CoreEntityODataController<FileContent, Guid>
    {
        public FileContentController(ICoreService<FileContent> service, ICoreAuthInfo auth, ILogger<FileContentController> log) 
            : base(service, auth, log) { }
    }
}
