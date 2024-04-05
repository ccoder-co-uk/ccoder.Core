using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.Planning;
using System.Linq;
using System.Security;
using System.Threading.Tasks;

namespace cCoder.Core.Services
{
    public class CalendarService : CoreService<Calendar>
    {
        public CalendarService(ICoreDataContext db) : base(db) { }

        public override async Task DeleteAsync(object id)
        {
            var calendar = Get(id);

            if (!User.Can(calendar.AppId, "calendar_delete"))
                throw new SecurityException("Access Denied!");

            var events = Db.GetAll<CalendarEvent>(false)
                .Where(ce => ce.CalendarId == calendar.Id)
                .ToArray();

            await Db.DeleteAllAsync(events);
            await Db.DeleteAsync(calendar);
        }
    }
}