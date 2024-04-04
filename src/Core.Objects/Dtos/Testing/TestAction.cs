using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace cCoder.Core.Objects.Dtos.Testing
{
    public abstract class TestAction
    {
        [Key]
        public string Name { get; set; }

        public string Setup { get; set; }

        public string Assertions { get; set; }

        public abstract Task Execute(IDictionary<string, object> context);
    }
}