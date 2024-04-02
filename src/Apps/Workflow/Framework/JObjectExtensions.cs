using Newtonsoft.Json.Linq;

namespace Workflow.Framework
{
    public static class JObjectExtensions
    {
        // for use with dynamic selections where the parsed results "might" be an object or might be an array
        public static TResult[] JObjectOrJArray<T, TResult>(this object source, Func<T, TResult> select) => source switch
        {
            JArray a => a.JArray(select),
            JObject jo => jo.JObject(select),
            _ => Array.Empty<TResult>()
        };

        static TResult[] JArray<T, TResult>(this JArray source, Func<T, TResult> select) => source.Cast<T>().Select(select).ToArray();

        static TResult[] JObject<T, TResult>(this JObject source, Func<T, TResult> select) => new[] { source }.Cast<T>().Select(select).ToArray();
    }
}