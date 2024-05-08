using cCoder.Core.Objects;

namespace Scheduler;

public class SystemAuthInfo : ICoreAuthInfo
{
    public string SSOUserId { get; set; } = "Guest";
    public string Token { get; set; }
}