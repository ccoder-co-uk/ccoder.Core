using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.Planning;
using cCoder.Core.Services;

namespace cCoder.Core.Api.Controllers
{
    public class CalendarEventController : CoreEntityODataController<CalendarEvent, int>
    {
        public CalendarEventController(ICoreService<CalendarEvent> service, ICoreAuthInfo auth, ILogger<CalendarEventController> log) 
            : base(service, auth, log) { }
    }
}