using Core.Objects;
using Core.Objects.Entities.Planning;
using Core.Services;

namespace Core.Api.Controllers
{
    public class BackgroundJobController : CoreEntityODataController<BackgroundJob, int>
    {
        public BackgroundJobController(ICoreService<BackgroundJob> service, ICoreAuthInfo auth, ILogger<BackgroundJobController> log) 
            : base(service, auth, log) { }
    }
}