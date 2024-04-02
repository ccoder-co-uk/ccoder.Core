using Core.Objects;
using Core.Objects.Entities.Mail;
using Core.Services;

namespace Core.Api.Controllers
{
    public class MailServerController : CoreEntityODataController<MailServer, int>
    {
        public MailServerController(ICoreService<MailServer> service, ICoreAuthInfo auth, ILogger<MailServerController> log) 
            : base(service, auth, log) { }
    }
}