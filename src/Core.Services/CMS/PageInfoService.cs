using Core.Objects;
using Core.Objects.Entities.CMS;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Services.CMS
{
    public class PageInfoService : CoreService<PageInfo>, ICoreService<PageInfo>
    {
        public PageInfoService(ICoreDataContext db) : base(db) { }

        public override async Task<PageInfo> UpdateAsync(PageInfo entity)
        {
            var result = await base.UpdateAsync(entity);

            await HandlePageInfoUpdateEvent(entity);

            return result;
        }

        private async ValueTask HandlePageInfoUpdateEvent(PageInfo pageInfo)
        {
            if (pageInfo.CultureId != string.Empty)
                return;

            var page = Db.GetAll<Page>(false)
                .FirstOrDefault(p => p.Id == pageInfo.PageId);

            page.Path = pageInfo.Title.Replace(" ", "");

            if (page.ParentId != null)
            {
                var parentPagePath = Db.GetAll<Page>(false)
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
            var childPages = Db.GetAll<Page>(false)
                .Where(p => p.ParentId == parentPage.Id)
                .ToArray();

            foreach (var childPage in childPages)
            {
                var childPageInfoTitle = Db.GetAll<PageInfo>(false)
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
}
