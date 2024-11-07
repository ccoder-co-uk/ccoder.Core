using cCoder.Core.Objects.Entities.Planning;
using Microsoft.EntityFrameworkCore;

namespace cCoder.Core.Data;

public partial class CoreDataContext
{
    // Planning
    public virtual DbSet<Calendar> Calendars { get; set; }
    public virtual DbSet<CalendarEvent> Events { get; set; }
    public virtual DbSet<ScheduledTask> ScheduledTasks { get; set; }
}