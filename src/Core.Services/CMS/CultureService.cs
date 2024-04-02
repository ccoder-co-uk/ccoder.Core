using Core.Objects;
using Core.Objects.Entities.CMS;

namespace Core.Services
{
    public class CultureService : CoreService<Culture>
    {
        public CultureService(ICoreDataContext db) : base(db) { }
    }
}