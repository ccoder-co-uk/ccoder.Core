using cCoder.Security.Data.EF.Interfaces;
using cCoder.Security.Objects.Entities;
using HostedServices.Services.Scheduled.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HostedServices.Services.Scheduled.Tasks;

public sealed class TokenCleaner(IServiceScope scope, ILogger<TokenCleaner> log) : IScheduled1MinuteOperation
{
    public async Task Run()
    {
        var ssoDbFactory = scope.ServiceProvider.GetService<ISecurityDbContextFactory>();
        using var sso = ssoDbFactory.CreateDbContext();

        Token[] expiredTokens = sso.Tokens
            .IgnoreQueryFilters()
            .Where(t => t.Expires < DateTimeOffset.UtcNow)
            .ToArray();

        if (expiredTokens.Length != 0)
        {
            sso.RemoveRange(expiredTokens);
            await sso.SaveChangesAsync();

            log.LogDebug($"{expiredTokens.Length} Expired tokens removed");
        }
    }
}