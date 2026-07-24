// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Models;

namespace cCoder.Core.Services.Setup;

internal interface IFirstTimeSetupUserService
{
    Task AuthenticateBootstrapUserAsync(
        string userId,
        string password,
        CancellationToken cancellationToken = default);

    Task EnsureBootstrapCoreUserAsync(
        FirstTimeSetupBootstrapUser bootstrapUser,
        CancellationToken cancellationToken = default);

    Task CompleteFirstUserRegistrationAsync(
        FirstTimeSetupRequest request,
        FirstTimeSetupBootstrapUser bootstrapUser,
        int appId,
        CancellationToken cancellationToken = default);

    Task RollbackAsync(
        string bootstrapUserId,
        CancellationToken cancellationToken = default);
}