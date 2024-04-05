using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.Mail;
using cCoder.Core.Services;

namespace cCoder.Core.Api.Controllers
{
    public class SentEmailController : CoreEntityODataController<SentEmail, int>
    {
        public SentEmailController(ICoreService<SentEmail> service, ICoreAuthInfo auth, ILogger<SentEmailController> log) 
            : base(service, auth, log) { }
    }
}