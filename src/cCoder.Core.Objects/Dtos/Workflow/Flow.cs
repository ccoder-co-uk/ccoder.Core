using cCoder.Core.Objects.Workflow.Activities;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace cCoder.Core.Objects.Dtos.Workflow
{
    public class Flow
    {
        [Key]
        [Required]
        public string Name { get; set; }

        public string RequiredRoles { get; set; }

        public Activity[] Activities { get; set; }

        public Link[] Links { get; set; }

        public T GetActivity<T>(string withRef) where T : Activity => (T)Activities.FirstOrDefault(a => a.Ref == withRef);
    }
}