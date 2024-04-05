using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.Logging;
using cCoder.Core.Services;

namespace cCoder.Core.Api.Controllers
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