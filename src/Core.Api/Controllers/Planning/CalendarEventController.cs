using Core.Objects;
using Core.Objects.Entities.Planning;
using Core.Services;

namespace Core.Api.Controllers
{
    public class CalendarEventController : CoreEntityODataController<CalendarEvent, int>
    {
        public CalendarEventController(ICoreService<CalendarEvent> service, ICoreAuthInfo auth, ILogger<CalendarEventController> log) 
            : base(service, auth, log) { }
    }
}