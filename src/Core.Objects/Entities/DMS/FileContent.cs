using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace cCoder.Core.Objects.Entities.DMS
{
    [Table("FileContents", Schema = "DMS")]
    [ApiIgnore]
    [Parent("File")]
    public class FileContent
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [ForeignKey("File")]
        public Guid FileId { get; set; }

        public string Description { get; set; }

        [StringLength(10)]
        public string Size { get; set; }

        public string CreatedBy { get; set; }

        public DateTimeOffset CreatedOn { get; set; }

        public int Version { get; set; }

        public byte[] RawData { get; set; }

        public File File { get; set; }
    }
}