using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.Planning;
using System.Threading.Tasks;

namespace cCoder.Core.Services
{
    public class BackgroundJobService : CoreService<BackgroundJob>
    {
        public BackgroundJobService(ICoreDataContext db) : base(db) { }

        public override Task<BackgroundJob> AddAsync(BackgroundJob entity)
        {
            entity.CreatedBy = AuthInfo.SSOUserId;
            entity.CreatedOn = System.DateTimeOffset.Now;
            entity.LastUpdated = System.DateTimeOffset.Now;
            return base.AddAsync(entity);
        }
    }
}
