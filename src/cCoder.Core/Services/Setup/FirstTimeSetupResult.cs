namespace cCoder.Core.Services.Setup;

public sealed record FirstTimeSetupResult(
    string TenantId,
    int AppId,
    string UserId
);
