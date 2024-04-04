using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.CMS;

namespace cCoder.Core.Services
{
    public class CultureService : CoreService<Culture>
    {
        public CultureService(ICoreDataContext db) : base(db) { }
    }
}