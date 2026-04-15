using CoreApp = cCoder.Data.Models.CMS.App;
using SecurityUser = cCoder.Data.Models.Security.User;


namespace cCoder.Core.Services.Orchestrations;

public interface ICMSUserRegistrationOrchestrationService
{
    ValueTask<SecurityUser> RegisterUserAsync(
        SecurityUser user,
        int appId,
        string confirmationToken,
        bool sendConfirmationEmail = true
    );

    ValueTask<SecurityUser> InviteUserAsync(SecurityUser user, int appId, string invitationToken);

    ValueTask SendInvitationEmailAsync(string invitationToken, CoreApp app, SecurityUser user);

    ValueTask ResendUserInviteEmailAsync(string userId, int appId, string invitationToken);
}


