// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.Core.Services.Setup;

public sealed class FirstTimeSetupResult(
    string tenantId,
    int appId,
    string userId)
{
    public string TenantId { get; } = tenantId;

    public int AppId { get; } = appId;

    public string UserId { get; } = userId;
}