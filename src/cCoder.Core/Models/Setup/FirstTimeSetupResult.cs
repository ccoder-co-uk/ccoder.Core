// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.Core.Services.Setup;

public sealed class FirstTimeSetupResult
{
    public FirstTimeSetupResult(
        string tenantId,
        int appId,
        string userId)
    {
        TenantId = tenantId;
        AppId = appId;
        UserId = userId;
    }

    public string TenantId { get; }

    public int AppId { get; }

    public string UserId { get; }
}