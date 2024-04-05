using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace cCoder.Core.Objects.Entities.CMS
{
    [Table("Contents", Schema = "CMS")]
    [ApiIgnore]
    [Parent("Page")]
    public class Content
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("Page")]
        public int PageId { get; set; }

        [ForeignKey("Culture")]
        [Required(AllowEmptyStrings = true)]
        public string CultureId { get; set; }

        [Required]
        public string Name { get; set; }
        public string Html { get; set; }
        public virtual Culture Culture { get; set; }
        public virtual Page Page { get; set; }
    }
}