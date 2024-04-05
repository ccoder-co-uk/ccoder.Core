using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.CMS;
using cCoder.Core.Services;

namespace cCoder.Core.Api.Controllers
{
    public class CultureController : CoreEntityODataController<Culture, string>
    {
        protected new ICoreService<Culture> Service => 
            base.Service as ICoreService<Culture>;

        public CultureController(ICoreService<Culture> service, ICoreAuthInfo auth, ILogger<CultureController> log) 
            : base(service, auth, log) { }
    }
}