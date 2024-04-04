using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace cCoder.Core.Objects.Entities.Logging
{
    [Table("LogEntries", Schema = "Logging")]
    [ApiIgnore]
    public class LogEntry
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int Level { get; set; }

        [Required]
        public string Message { get; set; }

        [Required]
        public string AppName { get; set; }

        [Required]
        public string TypeName { get; set; }

        public DateTime Date { get; set; }

        public virtual IEnumerable<LogDataItem> Data { get; set; }

        public LogEntry() { }

        public LogEntry(LoggingLevel level) => Level = (int)level;
    }



    public enum LoggingLevel
    {
        Error,
        Info,
        Warning,
        Debug
    }
}
