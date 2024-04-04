using cCoder.Core.Objects.Entities.CMS;
using cCoder.Core.Objects.Entities.Security;

namespace cCoder.Core.Objects.Dtos
{
    public abstract class RenderParams
    {
        public App App { get; }
        public User User { get; }
        public string Culture { get; set; }

        protected RenderParams(App app, User user) : this(app, user, "") { }

        protected RenderParams(App app, User user, string culture)
        {
            App = app;
            User = user;
            Culture = culture;
        }
    }

    public class ComponentRenderParams : RenderParams
    {
        public ICoreDataContext Db { get; }

        public string Theme { get; }

        public ComponentRenderParams(ICoreDataContext ctx, string theme, App app, User user, string culture)
            : base(app, user, culture)
        {
            Db = ctx;
            Theme = theme ?? "Default";
        }
    }

    public class PageRenderParams : ComponentRenderParams
    {
        public Page Page { get; }
        public bool Edit { get; }

        public PageRenderParams(ICoreDataContext ctx, Page page, string theme, App app, User user, string culture, bool edit = false)
            : base(ctx, theme, app, user, culture)
        {
            Page = page;
            Edit = edit;
        }
    }

    public class TemplateRenderParams : RenderParams
    {
        public TemplateRenderParams(App app, User user, string culture) : base(app, user, culture) { }
    }
}