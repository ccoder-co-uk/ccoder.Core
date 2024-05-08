using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.CMS;

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

        Page page = Db.GetAll<Page>(false)
            .FirstOrDefault(p => p.Id == pageInfo.PageId);

        if(page.Path == string.Empty)
            page.Path = pageInfo.Title.Replace(" ", "");

        if (page.ParentId != null)
        {
            string parentPagePath = Db.GetAll<Page>(false)
                .Where(p => p.Id == page.ParentId)
                .Select(p => p.Path)
                .FirstOrDefault();

            page.Path = parentPagePath + "/" + page.Path;
        }

        page.LastUpdated = DateTimeOffset.Now;
        page.LastUpdatedBy = User.Id;

        await Db.UpdateAsync(page);

        await HandleParentPageUpdate(page);
    }

    private async ValueTask HandleParentPageUpdate(Page parentPage)
    {
        Page[] childPages = Db.GetAll<Page>(false)
            .Where(p => p.ParentId == parentPage.Id)
            .ToArray();

        foreach (Page childPage in childPages)
        {
            string childPageInfoTitle = Db.GetAll<PageInfo>(false)
                .Where(pi => pi.PageId == childPage.Id && pi.CultureId == string.Empty)
                .Select(p => p.Title)
                .FirstOrDefault();

            childPage.Path = parentPage.Path + "/" + childPageInfoTitle.Replace(" ", "");

            childPage.LastUpdated = DateTimeOffset.Now;
            childPage.LastUpdatedBy = User.Id;

            await Db.UpdateAsync(childPage);

            await HandleParentPageUpdate(childPage);
        }
    }
}
