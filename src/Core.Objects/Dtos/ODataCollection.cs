using Newtonsoft.Json;
using System.Collections.Generic;

namespace Core.Objects.Dtos
{
    public class ODataCollection<TCollectionType>
    {
        [JsonProperty("@odata.context")]
        public string ODataContext { get; set; }

        public IEnumerable<TCollectionType> Value { get; set; }
    }
}
