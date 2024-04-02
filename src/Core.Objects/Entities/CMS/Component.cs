using Core.Objects.Dtos;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Objects.Entities.CMS
{
    [Table("Components", Schema = "CMS")]
    public class Component : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("App")]
        public int AppId { get; set; }

        public string ResourceKey { get; set; }
        public string Content { get; set; }
        public string Script { get; set; }

        public string Key { get; set; }
        public virtual App App { get; set; }

        public string Render(ComponentRenderParams p, Config config)
        {
            System.Collections.Generic.ICollection<Replacement> r = ContentHelper.DefaultReplacements(p, config);
            return $"<section name='{Name}' class='component' data-id='{Id}' data-resource-key='{ResourceKey}'>{ContentHelper.ProcessContentString(ResourceKey, p, Content, r)}<script type='text/javascript'>{ContentHelper.ProcessContentString(ResourceKey, p, Script, r)}</script></section>";
        }
    }
}