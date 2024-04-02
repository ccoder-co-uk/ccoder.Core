using Core.Objects;
using Core.Objects.Entities.CMS;
using Core.Services;

namespace Core.Api.Controllers
{

    public class ScriptController : CoreEntityODataController<Script, int>
    {
        protected new IScriptService Service => 
            base.Service as IScriptService;

        public ScriptController(IScriptService service, ICoreAuthInfo auth, ILogger<ScriptController> log) 
            : base(service, auth, log) { }
    }
}