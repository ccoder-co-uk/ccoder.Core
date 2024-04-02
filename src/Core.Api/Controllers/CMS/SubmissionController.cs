using Core.Objects;
using Core.Objects.Entities.CMS;
using Core.Services;

namespace Core.Api.Controllers
{
    public class SubmissionController : CoreEntityODataController<Submission, Guid>
    {
        public SubmissionController(ICoreService<Submission> service, ICoreAuthInfo auth, ILogger<SubmissionController> log) 
            : base(service, auth, log) { }
    }
}