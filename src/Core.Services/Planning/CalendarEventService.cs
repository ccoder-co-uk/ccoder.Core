using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.Planning;
using System.Threading.Tasks;

namespace cCoder.Core.Services.CMS
{
    public class CalendarEventService : CoreService<CalendarEvent>, ICoreService<CalendarEvent>
    {
        public CalendarEventService(ICoreDataContext db) : base(db) { }

        public override Task<CalendarEvent> AddAsync(CalendarEvent entity)
        {
            entity.Calendar = Db.Get<Calendar>(entity.CalendarId);
            return base.AddAsync(entity);
        }
    }
}