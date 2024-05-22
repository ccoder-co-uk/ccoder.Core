using System.Web;
using cCoder.Core.Objects;
using cCoder.Core.Objects.Dtos;
using cCoder.Core.Objects.Entities.CMS;
using cCoder.Core.Objects.Entities.Security;
using cCoder.Core.Services;
using Microsoft.EntityFrameworkCore;
using cCoder.Security.Api.Interfaces;
using Web.Services.Interfaces;

namespace Web.Services;

public class UserPasswordOrchestrationService : IUserPasswordOrchestrationService
{
    private readonly IAppService appService;
    private readonly ICoreDataContext coreContext;
    private readonly IUserRoleService userRoleService;
    private readonly IQueuedEmailService queuedEmailService;
    private readonly Config config;
    private readonly IAccountManager accountManager;

    public UserPasswordOrchestrationService(
        IAppService appService,
        ICoreDataContext coreContext,
        IUserRoleService userRoleService,
        IQueuedEmailService queuedEmailService,
        Config config,
        IAccountManager accountManager)
    {
        this.appService = appService;
        this.coreContext = coreContext;
        this.userRoleService = userRoleService;
        this.queuedEmailService = queuedEmailService;
        this.config = config;
        this.accountManager = accountManager;
    }

    public async ValueTask ChangePasswordAsync(User user, string oldPassword, string newPassword) =>
        await accountManager.ChangePasswordAsync(user.Id, oldPassword, newPassword);

    public async ValueTask ForgotPasswordAsync(string email, int appId)
    {
        var token = await accountManager.ForgotPasswordAsync(email);
        User user = coreContext.GetAll<User>()
            .IgnoreQueryFilters()
            .FirstOrDefault(dbUser => dbUser.Email == email);

        await this.ResetUserPassword(user, appId, token.Id);
    }

    public async ValueTask ResetUserPassword(User user, int appId, string token)
    {
        var app = appService.GetAll(false)
            .IgnoreQueryFilters()
            .Include(a => a.Roles)
            .ThenInclude(r => r.Users)
            .Include(a => a.Cultures)
            .Include(a => a.MailServers)
            .Include(a => a.Templates)
            .Include(a => a.Resources)
            .AsSplitQuery()
            .FirstOrDefault(a => a.Id == appId);

        await SendPasswordResetEmail(token, app, user);
    }

    public async ValueTask ConfirmForgotPasswordAsync(string token, string userId, string newPassword, string confirmNewPassword)
    {
        await accountManager.ConfirmForgotPasswordAsync(token, userId, newPassword, confirmNewPassword);
    }

    public async ValueTask SendPasswordResetEmail(string resetToken, App app, User user)
    {
        var template = app.Templates
            .FirstOrDefault(t => t.Name == "ForgotPassword");

        if (template == null || !app.MailServers.Any())
            return;

        var mailServer = app.MailServers
            .FirstOrDefault(s => s.Name == "Default")
                ??
            app.MailServers.FirstOrDefault();

        var renderModel = new
        {
            Token = resetToken,
            EncodedToken = HttpUtility.UrlEncode(resetToken),
            CoreUser = user
        };

        var renderParams = new TemplateRenderParams(app, user, user.DefaultCultureId);

        var passwordResetEmail = template
            .BuildEmailTo(
                user.Email,
                app.Name + ": Password Reset",
                renderParams, renderModel,
                mailServer,
                config);

        passwordResetEmail.SentByUserId = user.Id;

        await queuedEmailService.AddAsync(
            passwordResetEmail,
            checkPrivs: false);
    }
}