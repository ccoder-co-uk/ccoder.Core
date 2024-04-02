using Core.Objects.Entities.CMS;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Objects.Entities.Workflow
{
    [Table("BusinessProcesses", Schema = "Workflow")]
    public class BusinessProcess : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [ForeignKey("App")]
        public int AppId { get; set; }

        public string ReportingComponentName { get; set; }

        [Required]
        [StringLength(100)]
        public string Category { get; set; }

        public string DefinitionJson { get; set; }

        public virtual App App { get; set; }

        public virtual ICollection<FlowDefinition> Flows { get; set; }
    }
}