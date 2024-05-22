using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.Planning;
using HostedServices.Services.Scheduled.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HostedServices.Services.Scheduled.Tasks;

public sealed class TaskRunner(IServiceScope scope, ILogger<TaskRunner> log) : IScheduled1MinuteOperation
{
    public async Task Run()
    {
        using ICoreDataContext core = scope.ServiceProvider.GetService<ICoreDataContext>();

        int dueTasksExecuted = 0;
        ScheduledTask[] dueTasks = core.GetAll<ScheduledTask>(true)
            .IgnoreQueryFilters()
            .Where(t => t.NextExecution != null && t.NextExecution < DateTimeOffset.UtcNow && t.ScheduleInTicks != 0)
            .Include(t => t.Flow)
                .ThenInclude(f => f.App)
            .Include(t => t.ExecuteAsUser)
                .ThenInclude(u => u.Roles)
                    .ThenInclude(ur => ur.Role)
            .ToArray();

        if (dueTasks.Length == 0)
            return;

        IEnumerable<int> calendarIds = dueTasks.Where(d => d.ExcludedEventsCalendarId != null)
            .Select(d => d.ExcludedEventsCalendarId.Value);

        CalendarEvent[] events = core.GetAll<CalendarEvent>(false)
            .IgnoreQueryFilters()
            .Where(ce => calendarIds.Contains(ce.CalendarId) && ce.Start >= DateTimeOffset.Now.Date && ce.Start <= DateTimeOffset.Now.AddDays(14).Date)
            .ToArray();

        if (dueTasks.Length != 0)
        {
            log.LogInformation($"{dueTasks.Length} are scheduled to run, executing ...");

            foreach (ScheduledTask task in dueTasks)
            {
                log.LogDebug($"   Running task {task.Name} ({task.Id}), due to be run since @ {task.NextExecution:HH:mm:ss}");
                await RunTask(events, task, core);
                dueTasksExecuted++;
                log.LogDebug($"   Running task {task.Name} ({task.Id}) complete");
            }
        }

        log.LogInformation($"{dueTasksExecuted} Scheduled executions run.");
    }

    private async Task RunTask(CalendarEvent[] events, ScheduledTask task, ICoreDataContext core)
    {
        if (task.ExcludedEventsName != null)
        {
            string[] eventNames = task.ExcludedEventsName.Split(",");

            CalendarEvent[] matchedEvents = task.ExcludedEventsCalendarId != null
                ? events
                    .Where(e => e.CalendarId == task.ExcludedEventsCalendarId && eventNames.Contains(e.Name))
                    .ToArray()
                : [];

            if (matchedEvents.Any(e => e.Start.Date == DateTimeOffset.Now.Date))
                log.LogDebug($"Task {task.Id} - {task.Name} in app {task.AppId} skipped due to excluded date");
            else
                await task.Execute(core);
        }
        else
            await task.Execute(core);
    }
}