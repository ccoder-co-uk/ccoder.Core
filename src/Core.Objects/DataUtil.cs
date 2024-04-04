using cCoder.Core.Objects.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Dynamic;
using System.Text;
using System.Xml.Linq;

namespace cCoder.Core.Objects;

public static class Data
{
    private static readonly JTokenType[] primitives = new[] {
            JTokenType.String,
            JTokenType.Guid,
            JTokenType.Boolean,
            JTokenType.Integer,
            JTokenType.Date,
            JTokenType.Float,
            JTokenType.TimeSpan,
            JTokenType.Uri
    };

    public static ExpandoObject[] Flatten(object source, string path = "")
    {
        if (source is JArray array)
        {
            return array.SelectMany(i => Flatten(i, path)).ToArray();
        }

        List<ExpandoObject> results = new();
        IDictionary<string, JToken> obj = source as JObject ?? JObject.FromObject(source);

        // values for here
        KeyValuePair<string, JToken>[] values = obj
            .Where(kv => primitives.Contains(kv.Value.Type))
            .ToArray();


        results.AddRange(obj
            .Where(kv => !primitives.Contains(kv.Value.Type))
            .SelectMany(kv => Flatten(kv.Value, $"{path}_{kv.Key}".Trim("_".ToCharArray())))
        );

        // map the current object and return
        if (!results.Any())
        {
            IDictionary<string, object> thisObj = new ExpandoObject();
            values.ForEach(k => thisObj[$"{path}_{k.Key}"] = k.Value);
            results.Add((ExpandoObject)thisObj);
        }
        else // map properties for here to all children computed above
        {
            results.ForEach(r => values.ForEach(k => ((IDictionary<string, object>)r)[$"{path}_{k.Key}".Trim("_".ToCharArray())] = k.Value));
        }

        // return final set
        return results.ToArray();
    }

    public static T ParseXml<T>(string data)
    {
        StringBuilder builder = new();
        JsonSerializer.Create().Serialize(new CleanJsonWriter(new StringWriter(builder)), ParseXml(data));
        return JsonConvert.DeserializeObject<T>(builder.ToString());
    }

    public static XDocument ParseXml(string data) => 
        XDocument.Parse(data);

    public static T ParseJson<T>(string data) => 
        JsonConvert.DeserializeObject<T>(data, ObjectExtensions.GetJSONSettings());

    public static object ParseJson(string data) => 
        JsonConvert.DeserializeObject(data, ObjectExtensions.GetJSONSettings());

    public static IEnumerable<T> ParseCSV<T>(string data, CSVParseConfig config)
        where T : new() => 
            CSVParser<T>.Parse(data, config);
}