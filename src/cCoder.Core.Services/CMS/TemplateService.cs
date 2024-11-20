using cCoder.Core.Objects;
using cCoder.Core.Objects.Dtos;
using cCoder.Core.Objects.Entities.CMS;
using Microsoft.Extensions.Logging;

namespace cCoder.Core.Services.CMS;

public class TemplateService : CoreService<Template>, ITemplateService
{
    private readonly ILogger<TemplateService> log;
    private readonly Config config;

    public TemplateService(ICoreDataContext db, ILogger<TemplateService> log, Config config) : base(db)
    {
        this.log = log;
        this.config = config;
    }

    public string Render(int appId, string name, string culture, dynamic model)
    {
        try
        {
            App app = Db.GetAll<App>(false).FirstOrDefault(a => a.Id == appId);
            app.Templates = Db.GetAll<Template>(false).Where(t => t.AppId == app.Id).ToArray();
            app.Resources = Db.GetAll<Resource>(false).Where(t => t.AppId == app.Id).ToArray();
            TemplateRenderParams renderParams = new(app, User, culture);
            Template template = app.Templates.FirstOrDefault(t => t.Name.ToLower() == name.ToLower());
            return template.Render<dynamic>(model, renderParams, config);
        }
        catch (Exception ex)
        {
            log.LogError("A template failed to render because of the following exception:\n{Meesage}\n{StackTrace}", ex.Message, ex.StackTrace);
            throw;
        }
    }
}