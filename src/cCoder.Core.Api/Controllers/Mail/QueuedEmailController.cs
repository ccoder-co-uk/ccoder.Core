using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.Mail;
using cCoder.Core.Services;

namespace cCoder.Core.Api.Controllers.Mail;

public class QueuedEmailController : CoreEntityODataController<QueuedEmail, int>
{
    public QueuedEmailController(IQueuedEmailService service, ICoreAuthInfo auth, ILogger<QueuedEmailController> log)
        : base(service, auth, log) { }
}