using System.ComponentModel.DataAnnotations;
using System.Web;
using cCoder.AppSecurity.Services.Orchestrations;
using cCoder.Core.Services.Foundations.ContentManagement;
using CoreApp = cCoder.Data.Models.CMS.App;
using ContentTemplate = cCoder.Data.Models.CMS.Template;
using SecurityRole = cCoder.Data.Models.Security.Role;
using SecurityUser = cCoder.Data.Models.Security.User;
using SecurityUserRole = cCoder.Data.Models.Security.UserRole;

namespace cCoder.Core.Services.Orchestrations;

public class CMSUserRegistrationOrchestrationService(
    IContentManagementAppService contentManagementAppService,
    IRoleOrchestrationService roleOrchestrationService,
    IUserOrchestrationService userOrchestrationService,
    IUserRoleOrchestrationService userRoleOrchestrationService,
    ITemplatedEmailOrchestrationService templatedEmailOrchestrationService,
    ILogger<CMSUserRegistrationOrchestrationService> log
) : ICMSUserRegistrationOrchestrationService
{
    public async ValueTask<SecurityUser> RegisterUserAsync(
        SecurityUser user,
        int appId,
        string confirmationToken,
        bool sendConfirmationEmail = true
    )
    {
        try
        {
            CoreApp app = contentManagementAppService.Get(appId, true);
            SecurityRole usersRole = LoadUsersRole(appId);
            SecurityUser addedUser = await userOrchestrationService.AddAsync(user);

            if (
                usersRole is not null
                && !userRoleOrchestrationService.GetAll(true)
                    .Any(userRole => userRole.RoleId == usersRole.Id && userRole.UserId == user.Id)
            )
            {
                await userRoleOrchestrationService.SaveAsync(
                    new SecurityUserRole { RoleId = usersRole.Id, UserId = user.Id }
                );
            }

            if (sendConfirmationEmail)
                await SendConfirmRegistrationEmailAsync(confirmationToken, app, addedUser);

            return addedUser;
        }
        catch (Exception ex)
        {
            log.LogError("Failed to create user. {Message}", ex.Message);
            log.LogError(ex.StackTrace);
            throw;
        }
    }

    public async ValueTask<SecurityUser> InviteUserAsync(
        SecurityUser user,
        int appId,
        string invitationToken)
    {
        try
        {
            CoreApp app = contentManagementAppService.Get(appId, true);
            SecurityRole usersRole = LoadUsersRole(appId);
            SecurityUser addedUser = await userOrchestrationService.AddAsync(user);

            if (
                usersRole is not null
                && !userRoleOrchestrationService.GetAll(true)
                    .Any(userRole => userRole.RoleId == usersRole.Id && userRole.UserId == user.Id)
            )
            {
                await userRoleOrchestrationService.SaveAsync(
                    new SecurityUserRole { RoleId = usersRole.Id, UserId = user.Id }
                );
            }

            await SendInvitationEmailAsync(invitationToken, app, user);
            return addedUser;
        }
        catch (Exception ex)
        {
            log.LogError("Failed to create user. {Message}", ex.Message);
            log.LogError(ex.StackTrace);
            throw;
        }
    }

    public async ValueTask SendInvitationEmailAsync(
        string invitationToken,
        CoreApp app,
        SecurityUser user)
    {
        ContentTemplate template = app.Templates.FirstOrDefault(candidate =>
            candidate.Name == "AccessRequestApprovedEmail");

        if (template == null)
            return;

        var renderModel = new
        {
            Token = invitationToken,
            EncodedToken = HttpUtility.UrlEncode(invitationToken),
            CoreUser = new
            {
                user.Id,
                user.Email,
                user.DisplayName,
                user.DefaultCultureId,
                user.IsActive,
            },
        };

        await templatedEmailOrchestrationService.QueueAsync(
            app,
            template.Name,
            user.DefaultCultureId,
            renderModel,
            user.Email,
            app.Name + ": Confirm Invitation",
            user.Id
        );
    }

    public async ValueTask ResendUserInviteEmailAsync(string userId, int appId, string invitationToken)
    {
        SecurityUser user = userOrchestrationService.GetAll(true)
            .FirstOrDefault(candidate => candidate.Id == userId);
        CoreApp app = contentManagementAppService.Get(appId, true);

        if (user == null)
            throw new ValidationException("User not found");

        if (app == null)
            throw new ValidationException("App not found");

        await SendInvitationEmailAsync(invitationToken, app, user);
    }

    private async ValueTask SendConfirmRegistrationEmailAsync(
        string confirmationToken,
        CoreApp app,
        SecurityUser user
    )
    {
        ContentTemplate template = app.Templates.FirstOrDefault(candidate =>
            candidate.Name == "ConfirmRegistration");

        if (template == null)
            return;

        var renderModel = new
        {
            Token = confirmationToken,
            EncodedToken = HttpUtility.UrlEncode(confirmationToken),
            CoreUser = new
            {
                user.Id,
                user.Email,
                user.DisplayName,
                user.DefaultCultureId,
                user.IsActive,
            },
        };

        await templatedEmailOrchestrationService.QueueAsync(
            app,
            template.Name,
            user.DefaultCultureId,
            renderModel,
            user.Email,
            app.Name + ": Confirm Registration",
            user.Id
        );
    }

    private SecurityRole LoadUsersRole(int appId) =>
        roleOrchestrationService.GetAll(true).FirstOrDefault(role =>
            role.AppId == appId && role.Name == "Users");
}

