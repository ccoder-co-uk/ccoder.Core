using Core.Objects;
using Core.Objects.Entities.CMS;
using Core.Services;

namespace Core.Api.Controllers
{
    public class CultureController : CoreEntityODataController<Culture, string>
    {
        protected new ICoreService<Culture> Service => 
            base.Service as ICoreService<Culture>;

        public CultureController(ICoreService<Culture> service, ICoreAuthInfo auth, ILogger<CultureController> log) 
            : base(service, auth, log) { }
    }
}