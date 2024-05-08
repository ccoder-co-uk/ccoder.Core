using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.Planning;

namespace cCoder.Core.Services.Planning;

public class BackgroundJobService : CoreService<BackgroundJob>
{
    public BackgroundJobService(ICoreDataContext db) : base(db) { }

    public override Task<BackgroundJob> AddAsync(BackgroundJob entity)
    {
        entity.CreatedBy = AuthInfo.SSOUserId;
        entity.CreatedOn = DateTimeOffset.Now;
        entity.LastUpdated = DateTimeOffset.Now;
        return base.AddAsync(entity);
    }
}
