using System.Web;
using cCoder.AppSecurity.Services.Orchestrations;
using cCoder.Data;
using cCoder.Core.Services.Foundations.ContentManagement;
using cCoder.Core.Services.Orchestrations;
using cCoder.Data.Models.Security;
using cCoder.Security.Api.Interfaces;
using Web.Services.Interfaces;


namespace Web.Services;

public class UserPasswordOrchestrationService : IUserPasswordOrchestrationService
{
    private readonly IContentManagementAppService contentManagementAppService;
    private readonly IUserOrchestrationService userOrchestrationService;
    private readonly ITemplatedEmailOrchestrationService templatedEmailOrchestrationService;
    private readonly Config config;
    private readonly IAccountManager accountManager;

    public UserPasswordOrchestrationService(
        IContentManagementAppService contentManagementAppService,
        IUserOrchestrationService userOrchestrationService,
        ITemplatedEmailOrchestrationService templatedEmailOrchestrationService,
        Config config,
        IAccountManager accountManager)
    {
        this.contentManagementAppService = contentManagementAppService;
        this.userOrchestrationService = userOrchestrationService;
        this.templatedEmailOrchestrationService = templatedEmailOrchestrationService;
        this.config = config;
        this.accountManager = accountManager;
    }

    public async ValueTask ChangePasswordAsync(User user, string oldPassword, string newPassword) =>
        await accountManager.ChangePasswordAsync(user.Id, oldPassword, newPassword);

    public async ValueTask ForgotPasswordAsync(string email, int appId)
    {
        var token = await accountManager.ForgotPasswordAsync(email);

        User user = userOrchestrationService.GetByEmail(email, ignoreFilters: true);

        await this.ResetUserPasswordAsync(user, appId, token.Id);
    }

    public async ValueTask ResetUserPasswordAsync(User user, int appId, string token)
    {
        var app = contentManagementAppService.Get(appId, true);

        await SendPasswordResetEmailAsync(token, app, user);
    }

    public async ValueTask ConfirmForgotPasswordAsync(string token, string userId, string newPassword, string confirmNewPassword)
    {
        await accountManager.ConfirmForgotPasswordAsync(token, userId, newPassword, confirmNewPassword);
    }

    public async ValueTask SendPasswordResetEmailAsync(string resetToken, cCoder.Core.Models.App app, User user)
    {
        var template = app.Templates
            .FirstOrDefault(t => t.Name == "ForgotPassword");

        if (template == null)
            return;

        var renderModel = new
        {
            Token = resetToken,
            EncodedToken = HttpUtility.UrlEncode(resetToken),
            CoreUser = user
        };

        await templatedEmailOrchestrationService.QueueAsync(
            app,
            template.Name,
            user.DefaultCultureId,
            renderModel,
            user.Email,
            app.Name + ": Password Reset",
            user.Id
        );
    }
}










