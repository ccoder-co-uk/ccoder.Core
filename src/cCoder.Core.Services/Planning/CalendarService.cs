using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.Planning;
using System.Security;

namespace cCoder.Core.Services.Planning;

public class CalendarService : CoreService<Calendar>
{
    public CalendarService(ICoreDataContext db) : base(db) { }

    public override async Task DeleteAsync(object id)
    {
        Calendar calendar = Get(id);

        if (!User.Can(calendar.AppId, "calendar_delete"))
            throw new SecurityException("Access Denied!");

        CalendarEvent[] events = Db.GetAll<CalendarEvent>(false)
            .Where(ce => ce.CalendarId == calendar.Id)
            .ToArray();

        await Db.DeleteAllAsync(events);
        await Db.DeleteAsync(calendar);
    }
}