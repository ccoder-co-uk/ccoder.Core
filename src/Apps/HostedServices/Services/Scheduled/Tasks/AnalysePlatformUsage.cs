using cCoder.Core.Objects.Extensions;
using cCoder.Security.Data.EF;
using cCoder.Security.Data.EF.Interfaces;
using cCoder.Security.Objects.Entities;
using HostedServices.Services.Scheduled.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HostedServices.Services.Scheduled.Tasks;

internal sealed partial class AnalysePlatformUsage(IServiceScope scope) : IScheduledDailyOperation
{
    public async Task Run()
    {
        var ssoDbFactory = scope.ServiceProvider.GetService<ISecurityDbContextFactory>();
        using var sso = ssoDbFactory.CreateDbContext();

        List<DateTime> datesWithData = sso.UserEvents
            .IgnoreQueryFilters()
            .Select(e => e.CreatedOn.Date)
            .Distinct()
            .OrderByDescending(d => d)
            .ToList();

        if (datesWithData.FirstOrDefault() == DateTime.Today)
            datesWithData.RemoveAt(0);

        string[] tenants = sso.Tenants
            .IgnoreQueryFilters()
            .Select(t => t.Id)
            .ToArray();

        foreach (DateTime date in datesWithData)
        {
            IEnumerable<TenantAnalysis> reports = GenerateDailyReports(tenants, date, sso);
            sso.AddRange(reports);
            await sso.SaveChangesAsync();
        }

        string sql = $"DELETE UserEvents WHERE CreatedOn < '{DateTime.Today.AddDays(-2):yyyy-MM-dd}'";
        sso.Database.ExecuteSqlRaw(sql);
    }

    public IEnumerable<TenantAnalysis> GenerateDailyReports(string[] tenants, DateTime forDate, SecurityDbContext sso)
    {
        List<TenantAnalysis> results = new();

        foreach (string tenant in tenants)
            results.AddRange(GenerateUserActivityReport(tenant, forDate, sso));

        return results;
    }

    public IEnumerable<TenantAnalysis> GenerateUserActivityReport(string tenant, DateTime forDate, SecurityDbContext sso)
    {
        List<TenantAnalysis> results = new();

        TenantAnalysis existingReport = sso.TenantAnalysis
            .IgnoreQueryFilters()
            .FirstOrDefault(t =>
                t.TenantId == tenant &&
                t.CreatedOn == forDate &&
                t.Name == "User Activity (Daily)");

        if (existingReport == null)
            results.Add(new TenantAnalysis
            {
                TenantId = tenant,
                Key = "System",
                Name = "User Activity (Daily)",
                Value = AnalyseTenantUserActivity(tenant, forDate, sso).ToJsonForOdata(),
                CreatedOn = forDate
            });

        return results;
    }

    private object AnalyseTenantUserActivity(string tenantId, DateTime reportDate, SecurityDbContext sso)
    {
        UserActivity[] activityData = GetUserActivity(tenantId, reportDate, reportDate.AddDays(1), sso);

        var report = new
        {
            Users = AnalyseUserActivity(activityData),
            Pages = AnalysePageActivity(activityData),
            ApiCalls = AnalyseApiActivity(activityData)
        };

        return report;
    }

    private UserActivity[] GetUserActivity(string tenantId, DateTime from, DateTime to, SecurityDbContext sso) => sso.UserEvents
            .IgnoreQueryFilters()
            .Where(a => a.CreatedOn >= from && a.CreatedOn <= to && a.TenantId == tenantId)
            .Select(ue => new UserActivity
            {
                // tenant details
                TenantId = ue.TenantId,
                TenantName = ue.Tenant.Name,
                TenantDescription = ue.Tenant.Description,
                TenantCreatedBy = ue.Tenant.CreatedBy,
                TenantLastUpdatedBy = ue.Tenant.LastUpdatedBy,
                TenantCreatedOn = ue.Tenant.CreatedOn,
                TenantLastUpdated = ue.Tenant.LastUpdated,

                // user details
                UserId = ue.CreatedBy,
                UserDisplayName = ue.CreatedByUser.DisplayName,
                UserEmail = ue.CreatedByUser.Email,
                UserPhoneNumber = ue.TenantId,

                // event details
                EventId = ue.Id,
                EventName = ue.EventName,
                EventValue = ue.Value,
                EventCreatedOn = ue.CreatedOn,

                // session details
                SessionId = ue.SessionId
            })
            .ToArray();
}