using Core.Objects.Entities.CMS;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Objects.Entities.Planning
{
    [Table("BackgroundJobs", Schema = "Planning")]
    public class BackgroundJob
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("App")]
        public int AppId { get; set; }

        public string CreatedBy { get; set; }

        [StringLength(100)]
        public string State { get; set; } = "Queued";

        public string JobJson { get; set; }

        [StringLength(100)]
        public string OperationName { get; set; }

        public DateTimeOffset CreatedOn { get; set; }

        public DateTimeOffset LastUpdated { get; set; }

        public virtual App App { get; set; }
    }
}