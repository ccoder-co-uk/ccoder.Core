using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.CMS;

namespace cCoder.Core.Services
{
    public class LayoutService : CoreService<Layout>
    {
        public LayoutService(ICoreDataContext db) : base(db) { }
    }
}