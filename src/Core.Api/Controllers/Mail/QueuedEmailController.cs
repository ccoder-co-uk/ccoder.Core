using Core.Objects;
using Core.Objects.Entities.Mail;
using Core.Services;

namespace Core.Api.Controllers
{
    public class QueuedEmailController : CoreEntityODataController<QueuedEmail, int>
    {
        public QueuedEmailController(IQueuedEmailService service, ICoreAuthInfo auth, ILogger<QueuedEmailController> log) 
            : base(service, auth, log) { }
    }
}