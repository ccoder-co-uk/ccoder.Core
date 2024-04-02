using Core.Objects;
using Core.Objects.Entities.Logging;
using Core.Services;

namespace Core.Api.Controllers
{
    public class LogEntryController : CoreEntityODataController<LogEntry, int>
    {
        public LogEntryController(ICoreService<LogEntry> service, ICoreAuthInfo auth, ILogger<LogEntryController> log) 
            : base(service, auth, log) { }
    }

    public class LogDataItemController : CoreEntityODataController<LogDataItem, int>
    {
        public LogDataItemController(ICoreService<LogDataItem> service, ICoreAuthInfo auth, ILogger<LogDataItemController> log) 
            : base(service, auth, log) { }
    }
}