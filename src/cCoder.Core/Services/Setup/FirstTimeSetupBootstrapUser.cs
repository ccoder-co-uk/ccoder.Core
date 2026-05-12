namespace cCoder.Core.Services.Setup;

internal sealed record FirstTimeSetupBootstrapUser(
    string UserId,
    string Email,
    string DisplayName,
    string ConfirmationToken
);
