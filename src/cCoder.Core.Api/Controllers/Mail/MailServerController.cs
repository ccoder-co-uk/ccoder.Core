using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.Mail;
using cCoder.Core.Services;

namespace cCoder.Core.Api.Controllers.Mail;

public class MailServerController : CoreEntityODataController<MailServer, int>
{
    public MailServerController(ICoreService<MailServer> service, ICoreAuthInfo auth, ILogger<MailServerController> log)
        : base(service, auth, log) { }
}