using Core.Objects;
using Core.Objects.Entities.DMS;
using Core.Services;

namespace Core.Api.Controllers.DMS
{
    public class FileContentController : CoreEntityODataController<FileContent, Guid>
    {
        public FileContentController(ICoreService<FileContent> service, ICoreAuthInfo auth, ILogger<FileContentController> log) 
            : base(service, auth, log) { }
    }
}
