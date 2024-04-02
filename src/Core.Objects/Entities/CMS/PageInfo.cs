using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Objects.Entities.CMS
{
    [Table("PageInfo", Schema = "CMS")]
    [ApiIgnore]
    public class PageInfo
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Page")]
        public int PageId { get; set; }

        [ForeignKey("Culture")]
        [Required(AllowEmptyStrings = true)]
        public string CultureId { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        public string Keywords { get; set; }

        public virtual Page Page { get; set; }
        public virtual Culture Culture { get; set; }
    }
}
