using cCoder.Core.Objects.Entities.CMS;
using cCoder.Core.Objects.Entities.Security;
using cCoder.Core.Objects.Entities.Workflow;
using cCoder.Core.Objects.Extensions;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace cCoder.Core.Objects.Entities.Planning
{
    [Table("ScheduledTasks", Schema = "Planning")]
    public class ScheduledTask
    {
        // keys
        [Key]
        public int Id { get; set; }

        [ForeignKey("App")]
        public int AppId { get; set; }

        [ForeignKey("Flow")]
        public Guid FlowId { get; set; }

        [ForeignKey("ExcludedEventsCalendar")]
        public int? ExcludedEventsCalendarId { get; set; }
        public string ExcludedEventsName { get; set; }

        // details
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        public string ExecutionArgs { get; set; }
        public long ScheduleInTicks { get; set; }

        [NotMapped]
        [JsonIgnore]
        public TimeSpan? Schedule
        {
            get => TimeSpan.FromTicks(ScheduleInTicks);
        }

        // users
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }

        [ForeignKey("ExecuteAsUser")]
        public string ExecuteAs { get; set; }
        public virtual User ExecuteAsUser { get; set; }
        // points in time
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset LastUpdated { get; set; }
        public DateTimeOffset? LastExecuted { get; set; }
        public DateTimeOffset? NextExecution { get; set; }

        public virtual App App { get; set; }

        public virtual FlowDefinition Flow { get; set; }

        public virtual Calendar ExcludedEventsCalendar { get; set; }

        public async Task Execute(ICoreDataContext core, bool incrementNextExecution = true)
        {
            LastExecuted = DateTimeOffset.UtcNow;
            if (incrementNextExecution)
                while (NextExecution < DateTimeOffset.UtcNow && NextExecution != null)
                    NextExecution = ScheduleInTicks > 0 ? NextExecution + TimeSpan.FromTicks(ScheduleInTicks) : null;

            await core.SaveChangesAsync();

            if (ExecuteAsUser == null)
                throw new InvalidOperationException("User doesn't exist.");

            await Flow.QueueNewInstance(core, ExecuteAsUser, ExecutionArgs);
        }
    }
}