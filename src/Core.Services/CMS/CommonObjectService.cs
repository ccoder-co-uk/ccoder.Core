using cCoder.Core.Objects;
using cCoder.Core.Objects.Dtos;
using cCoder.Core.Objects.Entities;
using cCoder.Core.Objects.Entities.CMS;
using cCoder.Core.Objects.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;

namespace cCoder.Core.Services.CMS
{
    public class CommonObjectService : CoreService<CommonObject>, ICommonObjectService
    {
        protected ICommonObjectCache Cache { get; set; }

        public CommonObjectService(ICoreDataContext db, ICommonObjectCache cache) : base(db) { Cache = cache; }

        public IEnumerable<CommonObject> Latest(string type) => Cache.LatestSet.Where(r => r.Type == type);

        public async Task<IEnumerable<Result<CommonObject>>> Import(IEnumerable<CommonObject> items)
        {
            static bool matchesOnCultureNameAndKey(CommonObject dbc, CommonObject entry) => dbc.Culture == entry.Culture && dbc.Name == entry.Name && dbc.Key == entry.Key;

            CommonObject[] commonObjects = items as CommonObject[] ?? items.ToArray();
            IEnumerable<string> types = commonObjects.Select(i => i.Type).Distinct();

            List<Result<CommonObject>> results = new();

            List<CommonObject> adds = new();
            List<CommonObject> updates = new();

            foreach (string type in types)
            {
                IEnumerable<CommonObject> dbSet = Latest(type);
                CommonObject[] newSet = commonObjects.Where(i => i.Type == type).ToArray();

                foreach (CommonObject entry in newSet)
                {
                    CommonObject matchedDbEntry = dbSet.FirstOrDefault(dbc => matchesOnCultureNameAndKey(dbc, entry));
                    if (matchedDbEntry == null)
                    {
                        entry.Id = 0;
                        entry.Version = 1;
                        adds.Add(entry);
                    }
                    else if (entry.CreatedOn > matchedDbEntry.CreatedOn || entry.LastUpdated > matchedDbEntry.LastUpdated)
                    {
                        entry.Version = matchedDbEntry.Version + 1;
                        updates.Add(entry);
                    }
                }
            }

            results.AddRange(await AddAllAsync(adds));
            results.AddRange(await UpdateAllAsync(updates));
            return results;
        }

        public override async Task<CommonObject> AddAsync(CommonObject entity)
            => !User.Can(null, "commonobject_create") ? throw new SecurityException("Access Denied!") : await Db.AddAsync(entity);

        public override async Task<CommonObject> UpdateAsync(CommonObject entity)
        {
            if (!(User.Can(null, "commonobject_create") && User.Can(null, "commonobject_update")))
                throw new SecurityException("Access Denied!");

            int newVersionCount = Db.GetAll<CommonObject>(false).Count(c => c.Name == entity.Name && c.Type == entity.Type && c.Culture == entity.Culture && c.Key == entity.Key) + 1;
            int newVersionFromField = Db.GetAll<CommonObject>(false)
                .Where(c => c.Name == entity.Name && c.Type == entity.Type && c.Culture == entity.Culture && c.Key == entity.Key)
                .OrderByDescending(c => c.Version)
                .FirstOrDefault()?.Version ?? 1;

            entity.Id = 0;
            entity.Version = newVersionCount > newVersionFromField ? newVersionCount : newVersionFromField + 1;
            entity.CreatedOn = DateTimeOffset.Now;
            entity.LastUpdated = DateTimeOffset.Now;

            entity.LastUpdatedBy = Db.User.Id;
            entity.CreatedBy = Db.User.Id;

            entity = await Db.AddAsync(entity);

            if (entity.Type.ToLowerInvariant() == "core/component")
            {
                Cache.Set($"component|{entity.Name.ToLower()}", Objects.Data.ParseJson<Component>(entity.Json));
                var latestSetObject = Cache.LatestSet.First(r => r.Name.ToLowerInvariant() == entity.Name.ToLowerInvariant() && r.Type == "cCoder.Core/Component");
                latestSetObject.UpdateFrom(entity);
            }
            else if (entity.Type.ToLowerInvariant() == "core/resource")
            {
                Cache.Set($"resource|{entity.Key?.ToLower() ?? string.Empty}-{entity.Name?.ToLower() ?? string.Empty}-{entity.Culture?.ToLower() ?? string.Empty}", Objects.Data.ParseJson<Resource>(entity.Json));
                var latestSetObject = Cache.LatestSet.First(r => r.Name.ToLowerInvariant() == entity.Name.ToLowerInvariant()
                    && r.Key.ToLowerInvariant() == entity.Key.ToLowerInvariant() && r.Name == entity.Name.ToLowerInvariant()
                    && r.Culture.ToLowerInvariant() == entity.Culture.ToLowerInvariant()
                    && r.Type == "cCoder.Core/Resource");
                latestSetObject.UpdateFrom(entity);
            }
            else if (entity.Type.ToLowerInvariant() == "core/script")
            {
                var latestSetObject = Cache.LatestSet.First(r => r.Name.ToLowerInvariant() == entity.Name.ToLowerInvariant() && r.Type == "cCoder.Core/Script");
                latestSetObject.UpdateFrom(entity);
                Cache.Set($"script|{entity.Name.ToLower()}", Objects.Data.ParseJson<Script>(entity.Json));
            }



            return entity;
        }

        public override async Task DeleteAsync(object id)
        {
            if (!User.Can(null, "commonobject_delete"))
            {
                throw new SecurityException("Access Denied!");
            }

            _ = await Db.DeleteAsync(Db.Get<CommonObject>((int)id));
        }
    }
}