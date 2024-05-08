using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.Planning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Scheduler.Services;

public sealed class TaskRunner : IScheduledOperationRunner
{
    private readonly ICoreDataContext core;
    private readonly ILogger log;

    public TaskRunner(ICoreDataContext core, ILogger log)
    {
        this.core = core;
        this.log = log;
    }

    public async Task Run()
    {
        core.DisableFilters();

        int dueTasksExecuted = 0;
        ScheduledTask[] dueTasks = core.GetAll<ScheduledTask>(true)
            .Where(t => t.NextExecution != null && t.NextExecution < DateTimeOffset.UtcNow && t.ScheduleInTicks != 0)
            .Include(t => t.Flow)
                .ThenInclude(f => f.App)
            .ToArray();

        if (!dueTasks.Any())
        {
            return;
        }

        System.Collections.Generic.IEnumerable<int?> calendarIds = dueTasks.Where(d => d.ExcludedEventsCalendarId != null).Select(d => d.ExcludedEventsCalendarId);
        CalendarEvent[] events = core.GetAll<CalendarEvent>(false)
            .Where(ce => calendarIds.Contains(ce.CalendarId) && ce.Start >= DateTimeOffset.Now.Date && ce.Start <= DateTimeOffset.Now.AddDays(14).Date)
            .ToArray();

        foreach (ScheduledTask task in dueTasks)
        {
            await RunTask(events, task);
            dueTasksExecuted++;
        }

        log.LogDebug($"{dueTasksExecuted} Scheduled executions run.");
    }

    private async Task RunTask(CalendarEvent[] events, ScheduledTask task)
    {
        if (task.ExcludedEventsName != null)
        {
            string[] eventNames = task.ExcludedEventsName.Split(",");

            CalendarEvent[] matchedEvents = task.ExcludedEventsCalendarId != null
                ? events
                    .Where(e => e.CalendarId == task.ExcludedEventsCalendarId && eventNames.Contains(e.Name))
                    .ToArray()
                : Array.Empty<CalendarEvent>();

            if (matchedEvents.Any(e => e.Start.Date == DateTimeOffset.Now.Date))
                log.LogDebug($"Task {task.Id} - {task.Name} in app {task.AppId} skipped due to excluded date");
            else
                await task.Execute(core);
        }
        else
            await task.Execute(core);
    }

    public void Dispose() =>
        core.Dispose();
}