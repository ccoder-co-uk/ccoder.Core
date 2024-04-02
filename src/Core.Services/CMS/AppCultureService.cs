using Core.Objects;
using Core.Objects.Entities.CMS;
using System.Linq;
using System.Security;
using System.Threading.Tasks;

namespace Core.Services.CMS
{
    public class AppCultureService : CoreService<AppCulture>, ICoreService<AppCulture>
    {
        public AppCultureService(ICoreDataContext db) : base(db) { }

        public override async Task<AppCulture> AddAsync(AppCulture entity)
        {
            App app = Db.GetAll<App>(false)
                .FirstOrDefault(r => r.Id == entity.AppId);

            Culture culture = Db.GetAll<Culture>(true).FirstOrDefault(u => u.Id == entity.CultureId);

            if (app == null || culture == null)
                throw new SecurityException("Access Denied!");

            if (!User.Can(app.Id, "appculture_create") || !UserIsActive)
                throw new SecurityException("Access Denied!");

            return await Db.AddAsync(entity);
        }

        public override async Task DeleteAsync(object id)
        {
            AppCulture link = (AppCulture)id;

            AppCulture dbVersion = Db.GetAll<AppCulture>(false)
                .FirstOrDefault(ur => ur.AppId == link.AppId && ur.CultureId == link.CultureId);

            _ = dbVersion != null && User.Can(dbVersion.AppId, "appculture_delete")
                ? await Db.DeleteAsync(dbVersion)
                : throw new SecurityException("Access Denied!");
        }
    }
}