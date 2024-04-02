using Core.Objects.Entities.Planning;
using Microsoft.EntityFrameworkCore;

namespace Core.Data
{
    public partial class CoreDataContext
    {
        // Planning
        public virtual DbSet<Calendar> Calendars { get; set; }
        public virtual DbSet<CalendarEvent> Events { get; set; }
        public virtual DbSet<ScheduledTask> ScheduledTasks { get; set; }
        public virtual DbSet<BackgroundJob> BackgroundJobs { get; set; }
    }
}
