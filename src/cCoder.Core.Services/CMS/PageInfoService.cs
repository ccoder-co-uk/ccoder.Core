using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.CMS;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace cCoder.Core.Services.CMS;

public class PageInfoService : CoreService<PageInfo>, ICoreService<PageInfo>
{
    public PageInfoService(ICoreDataContext db, ILogger<PageInfoService> log) : base(db)
    {
        Log = log;
    }

    public ILogger<PageInfoService> Log { get; }

    public override async Task<PageInfo> UpdateAsync(PageInfo entity)
    {
        PageInfo result = await base.UpdateAsync(entity);

        await HandlePageInfoUpdateEvent(entity);

        return result;
    }

    private async ValueTask HandlePageInfoUpdateEvent(PageInfo pageInfo)
    {
        if (pageInfo.CultureId != string.Empty)
            return;

        Page page = await Db.GetAll<Page>(true)
            .Include(p => p.PageInfo)
            .Include(p => p.Parent)
            .FirstOrDefaultAsync(p => p.Id == pageInfo.PageId);

        try
        {
            page.RecomputePaths();
        }
        catch (Exception ex)
        {
            Log.LogDebug(ex.Message);
            Log.LogDebug(ex.StackTrace);

            if (ex.InnerException != null)
            {
                Log.LogDebug(ex.InnerException.Message);
                Log.LogDebug(ex.InnerException.StackTrace);
            }
        }


        page.LastUpdated = DateTimeOffset.Now;
        page.LastUpdatedBy = User.Id;

        await Db.UpdateAsync(page);
        await HandleParentPageUpdate(page);
    }

    private async ValueTask HandleParentPageUpdate(Page parentPage)
    {
        Page[] childPages = await Db.GetAll<Page>(true)
            .Where(p => p.ParentId == parentPage.Id)
            .Include(p => p.PageInfo)
            .Include(p => p.Parent)
            .ToArrayAsync();

        foreach (Page childPage in childPages)
        {
            childPage.RecomputePaths();

            await Db.UpdateAsync(childPage);
            await HandleParentPageUpdate(childPage);
        }
    }
}
