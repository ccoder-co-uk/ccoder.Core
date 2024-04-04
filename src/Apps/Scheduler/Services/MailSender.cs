using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.Mail;
using cCoder.Core.Objects.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Scheduler.Services
{
    public sealed class MailSender : IScheduledOperationRunner
    {
        ICoreDataContext Core { get; }

        readonly ILogger log;

        public MailSender(ICoreDataContext core, ILogger log)
        {
            Core = core;
            this.log = log;
            ServicePointManager.ServerCertificateValidationCallback = ValidateCertChain;
        }

        public async Task Run()
        {
            Core.DisableFilters();
            QueuedEmail[] queue = Core.GetAll<QueuedEmail>()
                .Include(e => e.FailedSends)
                .Include(e => e.App)
                    .ThenInclude(a => a.MailServers)
                .Where(e => e.FailedSends.Count < 10)
                .Take(100)
                .ToArray();

            if (queue.Any())
            {
                log.LogInformation($"Picked up a batch of {queue.Length} emails.");
                int success = 0;
                int failures = 0;
                using SmtpClient client = new() { EnableSsl = true };

                foreach (QueuedEmail email in queue)
                {
                    if (await ProcessEmail(client, email))
                    {
                        success++;
                    }
                    else
                    {
                        failures++;
                    }
                }

                if (success + failures > 0)
                {
                    log.LogInformation($"{success + failures} SMTP requests made of which {success} succeeded and {failures} failed.");
                }
            }
        }

        async Task<bool> ProcessEmail(SmtpClient client, QueuedEmail email)
        {
            MailServer server = email.App.MailServers.FirstOrDefault(s => s.Name == email.MailServerName);

            if (server == null)
            {
                _ = await Core.AddAsync(new EmailSendFailure { AttemptedOn = DateTimeOffset.UtcNow, EmailId = email.Id, FailureReason = "No mail server configuration could be found to send the email." });
                return false;
            }

            try
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
                    From = new MailAddress(server.User),
                    Subject = email.Subject,
                    Body = email.Content
                };
                msg.To.Add(email.To);

                // send to mail server
                client.Send(msg);

                // apply changes to the back end 
                SentEmail sendConfirmation = new SentEmail().UpdateFrom(email);
                sendConfirmation.SentOn = DateTimeOffset.UtcNow;
                sendConfirmation.From = server.User;

                _ = await Core.AddAsync(sendConfirmation);
                await Core.DeleteAllAsync(email.FailedSends);
                _ = await Core.DeleteAsync(email);
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

                _ = await Core.AddAsync(new EmailSendFailure { AttemptedOn = DateTimeOffset.UtcNow, EmailId = email.Id, FailureReason = reason.ToString() });
                return false;
            }
        }

        bool ValidateCertChain(object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            // If the cert is a valid, done!            // Some shared mail service providers return their root cert to your domain requests, allow this.
            if (sslPolicyErrors is SslPolicyErrors.None or SslPolicyErrors.RemoteCertificateNameMismatch)
                return true;

            // If there are errors in the certificate chain, look at each error to determine the cause.
            return (sslPolicyErrors & SslPolicyErrors.RemoteCertificateChainErrors) != 0 && AnalyseChain(certificate, chain);
        }

        static bool AnalyseChain(X509Certificate certificate, X509Chain chain)
        {
            if (chain != null && chain.ChainStatus != null)
            {
                X509ChainStatus status;

                for (int i = 0; i < chain.ChainStatus.Length; i++)
                {
                    status = chain.ChainStatus[i];

                    if (certificate.Subject == certificate.Issuer && status.Status == X509ChainStatusFlags.UntrustedRoot)
                        // Self-signed certificates with an untrusted root are valid. 
                        return true;
                    else if (status.Status != X509ChainStatusFlags.NoError)
                        // If there are any other errors in the certificate chain, the certificate is invalid,
                        // so the method returns false.
                        return false;
                }
            }

            // When processing reaches this line, the only errors in the certificate chain are 
            // untrusted root errors for self-signed certificates. These certificates are valid
            // for default Exchange server installations, so return true.
            return true;
        }

        public void Dispose() => Core.Dispose();
    }
}