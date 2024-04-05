using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.CMS;

namespace cCoder.Core.Services.CMS
{
    public class ScriptService : CoreService<Script>, IScriptService
    {
        public ScriptService(ICoreDataContext db) : base(db) { }
    }
}