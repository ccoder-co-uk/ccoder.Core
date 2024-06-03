using cCoder.Core.Objects;
using cCoder.Core.Objects.Dtos;
using cCoder.Core.Objects.Entities.CMS;
using cCoder.Core.Objects.Entities.Mail;
using cCoder.Core.Objects.Entities.Security;
using cCoder.Core.Objects.Extensions;
using cCoder.Core.Services.Orchestrations.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Web;

namespace cCoder.Core.Services.Orchestrations;

public class CMSUserRegistrationOrchestrationService(
    IAppService appService,
    ICoreService<User> coreUserService,
    IUserRoleService userRoleService,
    IQueuedEmailService queuedEmailService,
    Config config,
    ILogger<CMSUserRegistrationOrchestrationService> log) : ICMSUserRegistrationOrchestrationService
{
    public async ValueTask<User> RegisterUserAsync(User user, int appId, string confirmationToken, bool sendConfirmationEmail = true)
    {
        try
        {
            App app = appService.GetAll(false)
                .IgnoreQueryFilters()
                .Include(a => a.Roles)
                    .ThenInclude(r => r.Users)
                .Include(a => a.Cultures)
                .Include(a => a.MailServers)
                .Include(a => a.Templates)
                .Include(a => a.Resources)
                .AsSplitQuery()
                .FirstOrDefault(a => a.Id == appId);

            Role usersRole = app.Roles
                .FirstOrDefault(r => r.Name == "Users");

            User addedUser = await coreUserService.AddAsync(user);

            bool userIsNotAlreadyInUsersRole = !usersRole.Users
                .Select(ur => ur.UserId)
                .Contains(user.Id);

            if (usersRole != null && userIsNotAlreadyInUsersRole)
                await userRoleService.SaveAsync(new UserRole
                {
                    RoleId = usersRole.Id,
                    UserId = user.Id
                });

            if(sendConfirmationEmail)
                await SendConfirmRegistrationEmail(confirmationToken, app, addedUser);
    
            return addedUser;
        }
        catch
        (Exception ex)
        {
            log.LogError($"Failed to create user. {ex.Message}");
            log.LogError(ex.StackTrace);
            throw;
        }
    }

    public async ValueTask<User> InviteUserAsync(User user, int appId, string invitationToken)
    {
        try
        {
            App app = appService.GetAll(false)
                .IgnoreQueryFilters()
                .Include(a => a.Roles)
                    .ThenInclude(r => r.Users)
                .Include(a => a.Cultures)
                .Include(a => a.MailServers)
                .Include(a => a.Templates)
                .Include(a => a.Resources)
                .AsSplitQuery()
                .FirstOrDefault(a => a.Id == appId);

            Role usersRole = app.Roles
                .FirstOrDefault(r => r.Name == "Users");

            User addedUser = await coreUserService.AddAsync(user);

            bool userIsNotAlreadyInUsersRole = !usersRole.Users
                .Select(ur => ur.UserId)
                .Contains(user.Id);

            if (usersRole != null && userIsNotAlreadyInUsersRole)
                await userRoleService.SaveAsync(new UserRole
                {
                    RoleId = usersRole.Id,
                    UserId = user.Id
                });

            await SendInvitationEmail(invitationToken, app, addedUser);

            return addedUser;
        }
        catch
        (Exception ex)
        {
            log.LogError($"Failed to create user. {ex.Message}");
            log.LogError(ex.StackTrace);
            throw;
        }
    }

    public async ValueTask SendInvitationEmail(string invitationToken, App app, User user)
    {
        Template template = app.Templates
            .FirstOrDefault(t => t.Name == "Invitation");

        if (template == null || !app.MailServers.Any())
            return;

        MailServer mailServer = app.MailServers
            .FirstOrDefault(s => s.Name == "Default")
                ??
            app.MailServers.FirstOrDefault();

        var renderModel = new
        {
            Token = invitationToken,
            EncodedToken = HttpUtility.UrlEncode(invitationToken),
            CoreUser = new User().UpdateFrom(user, true)
        };

        TemplateRenderParams renderParams = new(app, user, user.DefaultCultureId);

        QueuedEmail confirmInvitationEmail = template
            .BuildEmailTo(
                user.Email,
                app.Name + ": Confirm Invitation",
                renderParams, renderModel,
                mailServer,
                config);

        confirmInvitationEmail.SentByUserId = user.Id;

        await queuedEmailService.AddAsync(
            confirmInvitationEmail,
            checkPrivs: false);
    }

    private async ValueTask SendConfirmRegistrationEmail(string confirmationToken, App app, User user)
    {
        Template template = app.Templates
            .FirstOrDefault(t => t.Name == "ConfirmRegistration");

        if (template == null || !app.MailServers.Any())
            return;

        MailServer mailServer = app.MailServers
            .FirstOrDefault(s => s.Name == "Default")
                ??
            app.MailServers.FirstOrDefault();

        var renderModel = new
        {
            Token = confirmationToken,
            EncodedToken = HttpUtility.UrlEncode(confirmationToken),
            CoreUser = new User().UpdateFrom(user)
        };

        TemplateRenderParams renderParams = new(app, user, user.DefaultCultureId);

        QueuedEmail confirmRegistrationEmail = template
            .BuildEmailTo(
                user.Email, 
                app.Name + ": Confirm Registration", 
                renderParams, renderModel, 
                mailServer, 
                config);

        confirmRegistrationEmail.SentByUserId = user.Id;

        await queuedEmailService.AddAsync(
            confirmRegistrationEmail, 
            checkPrivs: false);
    }
}