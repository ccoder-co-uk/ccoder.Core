// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.Core.Services.Setup;

internal sealed class FirstTimeSetupBootstrapUser(
    string userId,
    string email,
    string displayName,
    string confirmationToken)
{
    public string UserId { get; } = userId;

    public string Email { get; } = email;

    public string DisplayName { get; } = displayName;

    public string ConfirmationToken { get; } = confirmationToken;
}