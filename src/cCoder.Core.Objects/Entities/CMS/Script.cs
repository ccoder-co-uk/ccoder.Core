using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace cCoder.Core.Objects.Entities.CMS
{
    [Table("Scripts", Schema = "CMS")]
    public class Script : BaseEntity
    {
        [Key]
        public int Id { get; set; }
        public string Key { get; set; }
        public int AppId { get; set; }
        public string Content { get; set; }

        public virtual App App { get; set; }
    }
}