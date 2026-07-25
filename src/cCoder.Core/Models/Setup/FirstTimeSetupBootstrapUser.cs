// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.Core.Services.Setup;

internal sealed class FirstTimeSetupBootstrapUser
{
    public FirstTimeSetupBootstrapUser(
        string userId,
        string email,
        string displayName,
        string confirmationToken)
    {
        UserId = userId;
        Email = email;
        DisplayName = displayName;
        ConfirmationToken = confirmationToken;
    }

    public string UserId { get; }

    public string Email { get; }

    public string DisplayName { get; }

    public string ConfirmationToken { get; }
}