using cCoder.Core.Objects;
using cCoder.Core.Objects.Dtos;
using cCoder.Core.Objects.Dtos.Mail;
using cCoder.Core.Objects.Entities.CMS;
using cCoder.Core.Objects.Entities.Mail;
using cCoder.Core.Objects.Entities.Security;
using Microsoft.EntityFrameworkCore;
using System.Security;

namespace cCoder.Core.Services.Mail;

public class QueuedEmailService : CoreService<QueuedEmail>, IQueuedEmailService
{
    public QueuedEmailService(ICoreDataContext db) : base(db) { }

    public override Task<QueuedEmail> AddAsync(QueuedEmail entity) =>
        AddAsync(entity, false);

    public async Task<QueuedEmail> AddAsync(QueuedEmail email, bool checkPrivs) =>
        checkPrivs
            ? await base.AddAsync(email)
            : await Db.AddAsync(email);

    public override async Task DeleteAsync(object id)
    {
        int queuedEmailId = (int)id;

        QueuedEmail queuedEmail = GetAll(true)
            .Include(q => q.FailedSends)
            .FirstOrDefault(r => r.Id == queuedEmailId);

        if (queuedEmail == null)
            throw new SecurityException("Access Denied!");

        if (!User.Can(queuedEmail.AppId, "queuedemail_delete"))
            throw new SecurityException("Access Denied!");

        await Db.DeleteAllAsync(queuedEmail.FailedSends);
        await Db.DeleteAsync(queuedEmail);
    }

    public async Task<QueuedEmail> AddTemplatedEmailAsync(TemplatedEmailDetails details, User coreUser)
    {
        App app = GetAppForTemplatedEmail(details.SourceDomain, details.TemplateName);

        Template template = app.Templates
            .FirstOrDefault(t => t.AppId == app.Id && t.Name == details.TemplateName);

        MailServer mailServer = app.MailServers
            .FirstOrDefault(s => s.Name == "Default") ??
                app.MailServers.FirstOrDefault();

        var renderModel = new
        {
            Data = details.Model,
            CoreUser = coreUser
        };

        renderModel.CoreUser.DefaultCulture = new Culture { Id = details.Culture ?? app.DefaultCultureId };

        var renderParams = new TemplateRenderParams(
            app,
            renderModel.CoreUser,
            renderModel.CoreUser.DefaultCultureId);

        QueuedEmail email = template.BuildEmailTo(
                details.ToEmail,
                $"{app.Name}: {details.Subject}",
                renderParams,
                renderModel,
                mailServer);

        email.SentByUserId = renderModel.CoreUser?.Id;

        return await AddAsync(
            email,
            checkPrivs: false);
    }

    private App GetAppForTemplatedEmail(string sourceDomain, string templateName)
    {
        App app = Db.GetAll<App>(false)
            .Include(a => a.Templates)
            .Include(s => s.MailServers)
            .FirstOrDefault(a => a.Domain == sourceDomain);

        if (app == null)
            throw new Exception($"No app found for domain '{sourceDomain}'");

        if (!app.Templates.Any(t => t.Name == templateName))
            throw new Exception($"No template named '{templateName}' found for domain '{sourceDomain}'");

        if (!app.MailServers.Any())
            throw new Exception($"No mail server found for domain '{sourceDomain}'");

        return app;
    }
}