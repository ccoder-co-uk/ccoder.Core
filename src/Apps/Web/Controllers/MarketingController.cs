using cCoder.Core.Objects.Entities.Mail;
using cCoder.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    [Route("")]
    public class MarketingController : Controller
    {
        private readonly IQueuedEmailService emailService;
        private readonly IAppService appService;

        public MarketingController(IQueuedEmailService emailService, IAppService appService)
        {
            this.emailService = emailService;
            this.appService = appService;
        }

        public class ContactRequest
        { 
            public string name { get; set; }
            public string email { get; set; }
            public string subject { get; set; }
            public string message { get; set; }
        }

        public class SubscribeRequest
        {
            public string name { get; set; }
            public string email { get; set; }
        }

        [HttpPost("Contact")]
        public async ValueTask<IActionResult> Contact(ContactRequest details)
        {
            await emailService.AddAsync(new QueuedEmail
            {
                AppId = GetAppId(), 
                IsBodyHtml = false, 
                MailServerName = "default", 
                Subject = $"Contact Request From {details.email}: {details.subject}", 
                Content = details.message,
                To = "info@ccoder.co.uk"
            });

            return Ok("OK");
        }

        [HttpPost("Subscribe")]
        public async ValueTask<IActionResult> Subscribe(SubscribeRequest details)
        {
            await emailService.AddAsync(new QueuedEmail
            {
                AppId = GetAppId(),
                IsBodyHtml = false,
                MailServerName = "default",
                Subject = $"Subscription Request From {details.email}",
                Content = "Subscription Request",
                To = "info@ccoder.co.uk"
            });

            return Ok("OK");
        }

        int GetAppId() => appService
            .GetAll()
            .FirstOrDefault(a => a.Domain.ToLower() == Request.Host.Host.ToLower())?.Id ?? 0;
    }
}
