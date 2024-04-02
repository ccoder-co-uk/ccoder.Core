using Core.Objects.Entities.CMS;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Objects.Entities.Planning
{
    [Table("Calendars", Schema = "Planning")]
    public class Calendar
    {
        public int Id { get; set; }

        [ForeignKey("App")]
        public int AppId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public virtual App App { get; set; }

        public virtual ICollection<CalendarEvent> Events { get; set; }
    }
}
