using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace cCoder.Core.Objects.Entities.CMS
{
    [Table("Submissions", Schema = "CMS")]
    public class Submission
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [ForeignKey("App")]
        public int AppId { get; set; }

        public string CreatedBy { get; set; }
        public string LastUpdatedBy { get; set; }

        public DateTimeOffset CreatedOn { get; set; }
        public DateTimeOffset LastUpdatedOn { get; set; }

        public string SourceComponent { get; set; }

        public string State { get; set; }

        [Required]
        public string DataJson { get; set; }

        [NotMapped]
        [JsonIgnore]
        [ApiIgnore]
        public dynamic Data
        {
            get => Objects.Data.ParseJson<dynamic>(DataJson);
            set => DataJson = JsonConvert.SerializeObject(value);
        }

        public virtual App App { get; set; }
    }
}