using cCoder.Core.Objects;
using cCoder.Core.Objects.Dtos;
using cCoder.Core.Objects.Entities.CMS;
using Microsoft.EntityFrameworkCore;

namespace cCoder.Core.Services.CMS;

public class ComponentService : CoreService<Component>, IComponentService
{
    private readonly Config config;
    private readonly ICommonObjectCache commonCache;

    public ComponentService(ICoreDataContext db, ICommonObjectCache commonCache, Config config) : base(db)
    {
        this.config = config;
        this.commonCache = commonCache;
    }

    public string Render(int appId, string name, string culture, string theme)
    {
        culture ??= User.DefaultCultureId;
        App app = Db.GetAll<App>(false)
            .Include(a => a.Components)
            .Include(a => a.Resources)
            .Include(a => a.Scripts)
            .AsSplitQuery()
            .FirstOrDefault(a => a.Id == appId);

        Component component = app.Components
            .Where(c => c.AppId == appId)
            .FirstOrDefault(c => c.Name.ToLower() == name.ToLower());
        Component cacheComponent = commonCache.Get<Component>($"component|{name.ToLower()}");
        ComponentRenderParams p = new(Db, theme, app, User, culture);
        return component != null ? component.Render(p, config) : cacheComponent.Render(p, config);
    }
}