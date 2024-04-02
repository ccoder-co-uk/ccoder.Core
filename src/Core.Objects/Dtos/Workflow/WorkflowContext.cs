using System;
using System.Collections.Generic;

namespace Core.Objects.Dtos.Workflow
{
    /// <summary>
    /// Construct one of these to execute a task structure.
    /// The execution context is passed between tasks as they execute.
    /// </summary>
    public class WorkflowContext
    {
        public string ExecutionState { get; set; }

        public Guid InstanceId { get; set; }

        public Flow Flow { get; set; }

        public IDictionary<string, object> Variables { get; set; }

        public ICollection<WorkflowLogEntry> ExecutionLog { get; set; }

    }
}