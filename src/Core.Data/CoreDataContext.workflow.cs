using Core.Objects.Entities.Workflow;
using Microsoft.EntityFrameworkCore;
using System;

namespace Core.Data
{
    public partial class CoreDataContext
    {
        // Workflow (rebuild)
        public DbSet<BusinessProcess> BusinessProcesses { get; set; }
        public virtual DbSet<WorkflowEvent> WorflowEvents { get; set; }
        public virtual DbSet<FlowDefinition> FlowDefinitions { get; set; }
        public virtual DbSet<FlowInstanceData> FlowInstances { get; set; }

        public int FlushWFInstances(DateTimeOffset from)
            => Database.ExecuteSqlRaw($"DELETE workflow.FlowInstances WHERE [Start] < '{from:yyyy-MM-dd}'");
    }
}