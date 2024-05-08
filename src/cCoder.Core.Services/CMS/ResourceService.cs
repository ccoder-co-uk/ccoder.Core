using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.CMS;
using cCoder.Core.Objects.Extensions;

namespace cCoder.Core.Services.CMS;

public class ResourceService : CoreService<Resource>, IResourceService
{
    public ResourceService(ICoreDataContext db) : base(db) { }

    public IEnumerable<Resource> GetAll(string key, string culture, int appId)
        => Db.GetResourcesBy(appId, key, culture);

    public override Task<Resource> AddAsync(Resource entity)
    {
        entity.CreatedOn = DateTimeOffset.Now;
        entity.CreatedBy = User.Id;

        entity.LastUpdated = entity.CreatedOn;
        entity.LastUpdatedBy = User.Id;
        return base.AddAsync(entity);
    }

    public override Task<Resource> UpdateAsync(Resource entity)
    {
        entity.LastUpdated = DateTimeOffset.Now;
        entity.LastUpdatedBy = User.Id;
        return base.UpdateAsync(entity);
    }

    public override async Task DeleteAsync(object id)
    {
        Resource resource = Get(id);

        if (resource != null)
        {
            if (resource.Culture.IsNullOrEmpty())
            {
                Resource[] allVersions = GetAll(true)
                    .Where(r => r.AppId == resource.AppId && r.Key == resource.Key && r.Name == resource.Name)
                    .ToArray();

                await base.DeleteAllAsync(allVersions);
            }
            else
                await base.DeleteAsync(id);
        }
    }
}