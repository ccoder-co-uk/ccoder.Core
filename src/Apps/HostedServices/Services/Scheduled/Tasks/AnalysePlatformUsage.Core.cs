using cCoder.Security.Objects.Entities;

namespace HostedServices.Services.Scheduled.Tasks;

internal sealed partial class AnalysePlatformUsage
{
    private static object AnalyseUserActivity(IEnumerable<UserActivity> data) => data
            .GroupBy(i => i.UserId)
            .Select(g => new
            {
                User = new
                {
                    Id = g.Key,
                    g.First().UserEmail,
                    g.First().UserDisplayName
                },
                Sessions = g
                    .Select(i => i.SessionId)
                    .Distinct()
                    .Count(),
                PageRequests = g
                    .Where(i => i.EventName.StartsWith("Page_GET/") && !i.EventName.StartsWith("Page_GET/lib/"))
                    .Count(),
                ApiRequests = g
                    .Where(i => i.EventName.StartsWith("Api_GET/"))
                    .Count()
            })
            .OrderByDescending(i => i.PageRequests + i.ApiRequests)
            .Take(10);

    private static object AnalysePageActivity(IEnumerable<UserActivity> data) => data
            .Where(i => i.EventName.StartsWith("Page_GET/") && !i.EventName.StartsWith("Page_GET/lib/"))
            .GroupBy(i => i.EventValue.Split('?').First())
            .Select(g => new
            {
                Page = g.Key,
                Sessions = g
                    .Select(i => i.SessionId)
                    .Distinct()
                    .Count(),
                Hits = g.Count()
            })
            .OrderByDescending(i => i.Hits)
            .Take(10);

    private static object AnalyseApiActivity(IEnumerable<UserActivity> data) => data
            .Where(i => i.EventName.StartsWith("Api_"))
            .GroupBy(i => i.EventValue.Split('?').First())
            .Select(g => new
            {
                Endpoint = g.Key,
                Sessions = g
                    .Select(i => i.SessionId)
                    .Distinct()
                    .Count(),
                Hits = g.Count()
            })
            .OrderByDescending(i => i.Hits)
            .Take(10);
}