using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.Packaging;
using cCoder.Core.Objects.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cCoder.Core.Services.CMS
{
    public class PackageService : CoreService<Package>, IPackageService
    {
        protected IEnumerable<IPackageInstaller> ImportContexts { get; }

        public PackageService(ICoreDataContext db, IEnumerable<IPackageInstaller> importContexts)
            : base(db)
        {
            ImportContexts = importContexts;
        }

        public override async Task<Package> AddAsync(Package entity)
        {
            Package result = await base.AddAsync(entity);

            if (entity.Items != null && entity.Items.Any())
            {
                await Db.DeleteAllAsync(Db.GetAll<PackageItem>().Where(i => i.PackageId == result.Id).ToArray());
                entity.Items.ForEach(i => i.PackageId = result.Id);
                _ = await Db.AddAllAsync(entity.Items);
            }

            return result;
        }

        public override async Task<Package> UpdateAsync(Package entity)
        {
            Package result = await base.UpdateAsync(entity);
            if (entity.Items != null && entity.Items.Any())
            {
                await Db.DeleteAllAsync(Db.GetAll<PackageItem>().Where(i => i.PackageId == result.Id).ToArray());
                entity.Items.ForEach(i => i.PackageId = result.Id);
                _ = await Db.AddAllAsync(entity.Items);
            }

            return result;
        }


        public Task Import(int appId, string packageUrl, string remoteAuth)
        {
            throw new NotImplementedException();
            /*
             * Do We use this ?
             * It seems to be the only place in the codebase we nee the Token property on CoreAuthInfo, if we don't need this we should retire it.
             * 
            HttpClient api = new HttpClient(new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate })
                .WithAuthToken(remoteAuth ?? AuthInfo.Token);

            using (api)
                return api.GetAsync<Package>(packageUrl).ContinueWith(t => Task.WhenAll(ImportContexts.Select(c => c.Import(appId, t.Result))));
            */
        }

        public async Task Import(int appId, Package package)
        {
            foreach (IPackageInstaller context in ImportContexts)
                await context.Import(appId, package);
        }
    }
}