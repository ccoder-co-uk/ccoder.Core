using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.CMS;
using Microsoft.EntityFrameworkCore;

namespace cCoder.Core.Services.CMS;

public class PageInfoService : CoreService<PageInfo>, ICoreService<PageInfo>
{
    public PageInfoService(ICoreDataContext db) : base(db) { }

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
            .FirstOrDefaultAsync(p => p.Id == pageInfo.PageId);

        page.RecomputePaths();

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
            .ToArrayAsync();

        foreach (Page childPage in childPages)
        {
            childPage.RecomputePaths();

            await Db.UpdateAsync(childPage);
            await HandleParentPageUpdate(childPage);
        }
    }
}
