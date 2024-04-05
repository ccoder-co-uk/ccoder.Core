using cCoder.Core.Objects.Entities.CMS;
using System.Collections.Generic;

namespace cCoder.Core.Objects.Dtos
{
    public class TemplateModel<T>
    {
        public IEnumerable<Resource> Resources { get; set; }
        public T Model { get; set; }
    }
}
