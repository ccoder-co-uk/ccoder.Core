using cCoder.Core.Objects.Entities.Packaging;
using cCoder.Core.Objects.Entities.Planning;
using cCoder.Core.Objects.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace cCoder.Core.Services.Packaging.Importers;

public class CalendarEventImporter : CoreImporter<CalendarEvent>
{
    private readonly ILogger<CalendarEventImporter> log;
    private readonly ICoreService<Calendar> calendarService;

    public CalendarEventImporter(ILogger<CalendarEventImporter> log, ICoreService<CalendarEvent> service, ICoreService<Calendar> calendarService) 
        : base(service, "Core/CalendarEvent") 
    {
        this.log = log;
        this.calendarService = calendarService; 
    }

    public override async Task Import(int appId, PackageItem item)
    {
        ImportCalendarEventInfo[] calendarEventImportSet = item.Data.StartsWith("{") ?
            new[] { item.Unpack<ImportCalendarEventInfo>() }
            : item.Unpack<ImportCalendarEventInfo[]>();

        Calendar[] calendars = await calendarService
            .GetAll(false)
            .IgnoreQueryFilters()
            .Where(f => f.AppId == appId)
            .ToArrayAsync();

        string[] calendarEventNames = calendarEventImportSet
            .Select(l => l.Name)
            .ToArray();

        CalendarEvent[] dbCalendarEvents = await Service.GetAll(false)
            .IgnoreQueryFilters()
            .Where(c => c.Calendar.AppId == appId && calendarEventNames.Contains(c.Name))
            .Include(e => e.Calendar)
            .ToArrayAsync();

        List<CalendarEvent> calendarEventsToAdd = new();

        foreach(var calendarEventImportInfo in calendarEventImportSet)
        {
            CalendarEvent calendarEvent = 
                MapImportInfoToCalendarEvent(calendars, calendarEventImportInfo, dbCalendarEvents);

            if (calendarEvent.CalendarId == 0)
                continue;

            if (calendarEvent.Id == 0)
                calendarEventsToAdd.Add(calendarEvent);
        }

        log.LogDebug(
            "{NewCalendarEventCount} new calendar events provided for import from package",
            calendarEventImportSet.Length);

        log.LogDebug(
            "Importing {NewCalendarEventCount} new calendar events for calendars {Calendars}", 
            calendarEventsToAdd.Count, string.Join(",", 
            calendars.Select(c => c.Name)));

        _ = await Service.AddAllAsync(calendarEventsToAdd);
    }

    private CalendarEvent MapImportInfoToCalendarEvent(
        Calendar[] calendars, 
        ImportCalendarEventInfo calendarEventImportInfo, 
        CalendarEvent[] dbCalendarEvents) => new()
    {
        Id = Array.Find(dbCalendarEvents, j => calendarEventImportInfo.CalendarName == j.Calendar.Name && j.Name == calendarEventImportInfo.Name)?.Id ?? 0,
        CalendarId = Array.Find(calendars, p => p.Name == calendarEventImportInfo.CalendarName)?.Id ?? 0,
        Name = calendarEventImportInfo.Name,
        DurationInTicks = calendarEventImportInfo.DurationInTicks,
        Start = calendarEventImportInfo.Start,
        Description = calendarEventImportInfo.Description
    };

    class ImportCalendarEventInfo
    {
        public string CalendarName { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTimeOffset Start { get; set; }
        public long DurationInTicks { get; set; }
    }
}