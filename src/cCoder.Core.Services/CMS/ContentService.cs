using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.CMS;

namespace cCoder.Core.Services.CMS
{
    public class ContentService : CoreService<Content>, ICoreService<Content>
    {
        public ContentService(ICoreDataContext db) : base(db) { }
    }
}