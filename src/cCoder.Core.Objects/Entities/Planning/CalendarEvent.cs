using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace cCoder.Core.Objects.Entities.Planning
{
    [Table("Events", Schema = "Planning")]
    [Parent("Calendar")]
    public class CalendarEvent
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTimeOffset Start { get; set; }
        public long DurationInTicks { get; set; }

        [ForeignKey("Calendar")]
        public int CalendarId { get; set; }
        public virtual Calendar Calendar { get; set; }
    }
}