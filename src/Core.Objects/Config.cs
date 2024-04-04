using System.Collections.Generic;

namespace cCoder.Core.Objects
{
    public class Config
    {
        public IDictionary<string, string> Logging { get; set; }

        public IDictionary<string, string> ConnectionStrings { get; set; }
        public IDictionary<string, string> Settings { get; set; }
        public IDictionary<string, string> Services { get; set; }

        public bool DebugInfo { get; set; }
        public bool LogSQL { get; set; }
    }
}