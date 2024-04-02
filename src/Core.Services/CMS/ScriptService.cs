using Core.Objects;
using Core.Objects.Entities.CMS;

namespace Core.Services.CMS
{
    public class ScriptService : CoreService<Script>, IScriptService
    {
        public ScriptService(ICoreDataContext db) : base(db) { }
    }
}