using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace cCoder.Core.Objects.Dtos.Workflow
{
    public class WorkflowLogEntry
    {
        [Required]
        public string Level { get; set; }

        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

        [Required]
        public string Message { get; set; }

        public WorkflowLogEntry() { }

        public WorkflowLogEntry(WorkflowLogLevel level, string message)
        {
            Level = level.ToString();
            Message = message;
        }
    }
}