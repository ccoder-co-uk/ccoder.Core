using cCoder.Core.Objects;
using cCoder.Core.Objects.Dtos;
using cCoder.Core.Objects.Entities.CMS;

namespace cCoder.Core.Services.CMS;

public class TemplateService : CoreService<Template>, ITemplateService
{
    public TemplateService(ICoreDataContext db) : base(db) { }

    public Task<string> Render(int appId, string name, string culture, dynamic model)
    {
        App app = Db.GetAll<App>(false).FirstOrDefault(a => a.Id == appId);
        app.Templates = Db.GetAll<Template>(false).Where(t => t.AppId == app.Id).ToArray();
        app.Resources = Db.GetAll<Resource>(false).Where(t => t.AppId == app.Id).ToArray();
        TemplateRenderParams renderParams = new(app, User, culture);
        Template template = app.Templates.FirstOrDefault(t => t.Name.ToLower() == name.ToLower());
        return template.Render<dynamic>(model, renderParams, null);
    }
}