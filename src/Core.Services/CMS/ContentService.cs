using Core.Objects;
using Core.Objects.Entities.CMS;

namespace Core.Services.CMS
{
    public class ContentService : CoreService<Content>, ICoreService<Content>
    {
        public ContentService(ICoreDataContext db) : base(db) { }
    }
}