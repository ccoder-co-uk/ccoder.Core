using Core.Objects.Entities.Security;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Objects.Entities.Workflow
{
    [Table("WorkflowEvents", Schema = "Workflow")]
    [Parent("Flow")]
    public class WorkflowEvent
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public string Type { get; set; }

        public string EventContext { get; set; }

        public string CreatedBy { get; set; }
        public DateTimeOffset CreatedOn { get; set; }

        [ForeignKey("Flow")]
        public Guid FlowId { get; set; }

        public virtual FlowDefinition Flow { get; set; }

        [ForeignKey("ExecuteAsUser")]
        [Required]
        public string ExecuteAs { get; set; }

        public virtual User ExecuteAsUser { get; set; }
    }
}