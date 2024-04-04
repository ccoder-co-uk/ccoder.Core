using System.Collections.Generic;

namespace cCoder.Core.Objects.Dtos.Metadata
{
    public class OperationContainer
    {
        public string Name { get; set; }

        public string Url { get; set; }

        public string Definition { get; set; }

        public string HttpVerb { get; set; }

        public bool Queryable { get; set; }

        public MetadataContainer ReturnType { get; set; }

        public IDictionary<string, string> Parameters { get; set; }
    }
}