using System.Web;
using cCoder.AppSecurity.Services.Orchestrations;
using cCoder.Core.Services.Foundations.ContentManagement;
using cCoder.Data.Models.CMS;
using cCoder.Data.Models.Security;
using cCoder.Security.Exposures;

namespace cCoder.Core.Services.Orchestrations;

public class UserPasswordOrchestrationService(
    IContentManagementAppService contentManagementAppService,
    IUserOrchestrationService userOrchestrationService,
    ITemplatedEmailOrchestrationService templatedEmailOrchestrationService,
    IAccountManager accountManager)
    : IUserPasswordOrchestrationService
{
    public ValueTask ChangePasswordAsync(User user, string oldPassword, string newPassword) =>
        accountManager.ChangePasswordAsync(user.Id, oldPassword, newPassword);

    public async ValueTask ForgotPasswordAsync(string email, int appId)
    {
        var token = await accountManager.ForgotPasswordAsync(email);
        User user = userOrchestrationService.GetByEmail(email, ignoreFilters: true);

        await ResetUserPasswordAsync(user, appId, token.Id);
    }

    public async ValueTask ResetUserPasswordAsync(User user, int appId, string token)
    {
        App app = contentManagementAppService.Get(appId, true);
        await SendPasswordResetEmailAsync(token, app, user);
    }

    public ValueTask ConfirmForgotPasswordAsync(
        string token,
        string userId,
        string newPassword,
        string confirmNewPassword) =>
        accountManager.ConfirmForgotPasswordAsync(token, userId, newPassword, confirmNewPassword);

    public async ValueTask SendPasswordResetEmailAsync(
        string resetToken,
        App app,
        User user)
    {
        Data.Models.CMS.Template template = app.Templates
            .FirstOrDefault(candidate => candidate.Name == "ForgotPassword");

        if (template is null)
            return;

        var renderModel = new
        {
            Token = resetToken,
            EncodedToken = HttpUtility.UrlEncode(resetToken),
            CoreUser = user,
        };

        await templatedEmailOrchestrationService.QueueAsync(
            app,
            template.Name,
            user.DefaultCultureId,
            renderModel,
            user.Email,
            app.Name + ": Password Reset",
            user.Id);
    }
}

