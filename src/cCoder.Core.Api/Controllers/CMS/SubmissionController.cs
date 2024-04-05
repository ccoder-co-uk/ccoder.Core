using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.CMS;
using cCoder.Core.Services;

namespace cCoder.Core.Api.Controllers
{
    public class SubmissionController : CoreEntityODataController<Submission, Guid>
    {
        public SubmissionController(ICoreService<Submission> service, ICoreAuthInfo auth, ILogger<SubmissionController> log) 
            : base(service, auth, log) { }
    }
}