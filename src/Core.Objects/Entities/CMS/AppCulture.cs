using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Objects.Entities.CMS
{
    [Table("AppCultures", Schema = "CMS")]
    [ApiIgnore]
    public class AppCulture
    {
        [ForeignKey("App")]
        public int AppId { get; set; }

        [ForeignKey("Culture")]
        public string CultureId { get; set; }

        public virtual App App { get; set; }

        public virtual Culture Culture { get; set; }
    }
}