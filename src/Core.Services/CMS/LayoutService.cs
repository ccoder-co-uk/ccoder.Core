using Core.Objects;
using Core.Objects.Entities.CMS;

namespace Core.Services
{
    public class LayoutService : CoreService<Layout>
    {
        public LayoutService(ICoreDataContext db) : base(db) { }
    }
}