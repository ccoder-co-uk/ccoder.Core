using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.Mail;
using cCoder.Core.Objects.Extensions;
using HostedServices.Services.Scheduled.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace HostedServices.Services.Scheduled.Tasks;

public sealed class MailSender(IServiceScope scope, ILogger<MailSender> log) : IScheduled1MinuteOperation
{
    public async Task Run()
    {
        ServicePointManager.ServerCertificateValidationCallback = CertChainValidator.ValidateCertChain;

        using ICoreDataContext core = scope.ServiceProvider.GetService<ICoreDataContext>();

        QueuedEmail[] queue = core.GetAll<QueuedEmail>()
            .IgnoreQueryFilters()
            .Include(e => e.FailedSends)
            .Include(e => e.App)
                .ThenInclude(a => a.MailServers)
            .Where(e => e.FailedSends.Count < 10)
            .Take(10)
            .ToArray();

        if (queue.Length != 0)
        {
            log.LogInformation($"Picked up a batch of {queue.Length} emails.");
            int success = 0;
            int failures = 0;
            using SmtpClient client = new() { EnableSsl = true };

            foreach (QueuedEmail email in queue)
            {
                if (await ProcessEmail(client, email, core))
                    success++;
                else
                    failures++;

                Thread.Sleep(500);
            }

            if (success + failures > 0)
                log.LogInformation($"{success + failures} SMTP requests made of which {success} succeeded and {failures} failed.");
        }
    }

    private async Task<bool> ProcessEmail(SmtpClient client, QueuedEmail email, ICoreDataContext core)
    {
        MailServer server = email.App
            .MailServers
            .FirstOrDefault(s => s.Name == email.MailServerName);

        if (server == null)
        {
            EmailSendFailure failure = new()
            {
                AttemptedOn = DateTimeOffset.UtcNow,
                EmailId = email.Id,
                FailureReason = "No mail server configuration could be found to send the email."
            };

            await core.AddAsync(failure);
            return false;
        }

        try
        {
            SendEmail(client, email, server);
            await MarkEmailAsSent(email, core, server);
            return true;
        }
        catch (Exception ex)
        {
            StringBuilder reason = new(ex.Message);

            while (ex.InnerException != null)
            {
                ex = ex.InnerException;
                _ = reason.Append($"\n{ex.Message}");
            }

            EmailSendFailure exceptionFailure = new()
            {
                AttemptedOn = DateTimeOffset.UtcNow,
                EmailId = email.Id,
                FailureReason = reason.ToString()
            };

            await core.AddAsync(exceptionFailure);
            return false;
        }
    }

    private static async Task MarkEmailAsSent(QueuedEmail email, ICoreDataContext core, MailServer server)
    {
        // apply changes to the back end 
        SentEmail sendConfirmation = new SentEmail().UpdateFrom(email);
        sendConfirmation.SentOn = DateTimeOffset.UtcNow;
        sendConfirmation.From = server.User;

        await core.AddAsync(sendConfirmation);
        await core.DeleteAllAsync(email.FailedSends);
        await core.DeleteAsync(email);
    }

    private static void SendEmail(SmtpClient client, QueuedEmail email, MailServer server)
    {
        // configure the client for sending this email
        client.Host = server.Host;
        client.Port = server.Port;
        client.EnableSsl = server.EnableSSL;
        client.UseDefaultCredentials = false;
        client.Credentials = new NetworkCredential(server.User, server.Password);
        client.DeliveryMethod = SmtpDeliveryMethod.Network;

        // construct the message
        MailMessage msg = new()
        {
            IsBodyHtml = email.IsBodyHtml,
            Subject = email.Subject,
            Body = email.Content
        };

        if (server.FromEmail is not null)
            msg.From = new MailAddress(server.FromEmail);

        msg.From ??= server.User.Contains('@')
            ? new MailAddress(server.User)
            : null;

        msg.To.Add(email.To);

        // send to mail server
        client.Send(msg);
    }
}