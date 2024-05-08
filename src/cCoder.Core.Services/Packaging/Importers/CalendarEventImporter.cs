using cCoder.Core.Objects.Entities.Packaging;
using cCoder.Core.Objects.Entities.Planning;
using cCoder.Core.Objects.Extensions;

namespace cCoder.Core.Services.Packaging.Importers;

public class CalendarEventImporter : CoreImporter<CalendarEvent>
{
    private readonly ICoreService<Calendar> calendarService;
    public CalendarEventImporter(ICoreService<CalendarEvent> service, ICoreService<Calendar> calendarService) : base(service, "Core/CalendarEvent") { this.calendarService = calendarService; }

    public override async Task Import(int appId, PackageItem item)
    {
        ImportCalendarEventInfo[] calendarEventImportSet = item.Data.StartsWith("{") ?
            new[] { item.Unpack<ImportCalendarEventInfo>() }
            : item.Unpack<ImportCalendarEventInfo[]>();

        IEnumerable<Calendar> calendars = calendarService.GetAll(false)
            .Where(f => f.AppId == appId)
            .ToArray();

        string[] calendarEventNames = calendarEventImportSet.Select(l => l.Name).ToArray();

        IEnumerable<dynamic> dbCalendarEvents = Service.GetAll(false)
            .AsQueryable()
            .Where(c => c.Calendar.AppId == appId && calendarEventNames.Contains(c.Name))
            .Select(l => new { l.Id, l.Name, CalendarName = l.Calendar.Name })
            .ToArray();

        List<CalendarEvent> calendarEventsToAdd = new();

        calendarEventImportSet.ForEach(calendarEventImportInfo =>
        {
            CalendarEvent calendarEvent = MapImportInfoToCalendarEvent(calendarEventImportInfo);

            calendarEvent.CalendarId = calendars.FirstOrDefault(p => p.Name == calendarEventImportInfo.CalendarName)?.Id ?? 0;

            dynamic dbCalendarEvent = dbCalendarEvents.FirstOrDefault(j => calendarEventImportInfo.CalendarName == j.CalendarName && j.Name == calendarEvent.Name);
            calendarEvent.Id = dbCalendarEvent?.Id ?? 0;

            if (calendarEvent.CalendarId != 0)
                calendarEventsToAdd.Add(calendarEvent);
        });

        _ = await Service.AddAllAsync(calendarEventsToAdd.Where(i => i.Id == 0));
    }

    private CalendarEvent MapImportInfoToCalendarEvent(ImportCalendarEventInfo importCalendarEventInfo)
        => new()
        {
            Name = importCalendarEventInfo.Name,
            DurationInTicks = importCalendarEventInfo.DurationInTicks,
            Start = importCalendarEventInfo.Start,
            Description = importCalendarEventInfo.Description
        };

    public class ImportCalendarEventInfo
    {
        public string CalendarName { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTimeOffset Start { get; set; }
        public long DurationInTicks { get; set; }
    }
}