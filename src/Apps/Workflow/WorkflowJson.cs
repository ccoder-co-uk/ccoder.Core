using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Workflow;

internal static class WorkflowJson
{
    public static JsonSerializerSettings GetJsonSettings() =>
        new()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            TypeNameHandling = TypeNameHandling.Objects,
            Formatting = Formatting.None,
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            NullValueHandling = NullValueHandling.Ignore,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            ContractResolver = new DefaultContractResolver { IgnoreSerializableAttribute = true }
        };

    public static JsonSerializerSettings GetODataJsonSettings() =>
        new()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            TypeNameHandling = TypeNameHandling.None,
            Formatting = Formatting.None,
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            NullValueHandling = NullValueHandling.Ignore,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            ContractResolver = new DefaultContractResolver { IgnoreSerializableAttribute = true },
            MaxDepth = 4
        };

    public static string ToJson(this object value) =>
        JsonConvert.SerializeObject(value, Formatting.None, GetJsonSettings());

    public static string ToJsonForOdata(this object value) =>
        JsonConvert.SerializeObject(value, Formatting.None, GetODataJsonSettings());
}
