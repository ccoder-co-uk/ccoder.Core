using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.Planning;
using cCoder.Core.Services;

namespace cCoder.Core.Api.Controllers
{
    public class BackgroundJobController : CoreEntityODataController<BackgroundJob, int>
    {
        public BackgroundJobController(ICoreService<BackgroundJob> service, ICoreAuthInfo auth, ILogger<BackgroundJobController> log) 
            : base(service, auth, log) { }
    }
}