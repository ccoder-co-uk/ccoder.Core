using cCoder.Data.Models.Packaging;

namespace cCoder.Core.Exposures.Setup;

public static partial class UIBaseline
{
    public static Package[] Packages =>
    [
        Roles,
        Layouts,
        Templates,
        Resources,
        Pages,
        Workflows,
        Components,
        Scripts,
        PageRoles,
        FolderRoles,
        Calendars,
        CalendarEvents
    ];
}