using Core.Objects;
using Core.Objects.Entities.Mail;
using Core.Services;

namespace Core.Api.Controllers
{
    public class SentEmailController : CoreEntityODataController<SentEmail, int>
    {
        public SentEmailController(ICoreService<SentEmail> service, ICoreAuthInfo auth, ILogger<SentEmailController> log) 
            : base(service, auth, log) { }
    }
}