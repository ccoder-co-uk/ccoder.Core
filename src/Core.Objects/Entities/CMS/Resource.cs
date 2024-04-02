using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Objects.Entities.CMS
{
    [Table("Resources", Schema = "CMS")]
    public class Resource : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("App")]
        public int AppId { get; set; }

        [Required]
        public string Key { get; set; }

        [Required(AllowEmptyStrings = true)]
        public string Culture { get; set; }

        [Required]
        public string DisplayName { get; set; }

        [Required]
        public string ShortDisplayName { get; set; }

        public virtual App App { get; set; }
    }
}