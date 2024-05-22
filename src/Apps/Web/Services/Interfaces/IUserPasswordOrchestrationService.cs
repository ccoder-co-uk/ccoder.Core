using cCoder.Core.Objects.Entities.Security;

namespace Web.Services.Interfaces;

public interface IUserPasswordOrchestrationService
{
    ValueTask ResetUserPassword(User user, int appId, string token);
    ValueTask ChangePasswordAsync(User user, string oldPassword, string newPassword);
    ValueTask ConfirmForgotPasswordAsync(string token, string userId, string newPassword, string confirmNewPassword);
    ValueTask ForgotPasswordAsync(string email, int appId);
}