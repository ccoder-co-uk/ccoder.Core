using cCoder.Core.Objects.Extensions;
using Microsoft.AspNetCore.OData.Query.Wrapper;
using System.Collections;
using System.Linq.Dynamic.Core;

namespace cCoder.Core.Api.Formatters
{
    public static class FormatterODataHelper
    {
        public static object HandleOData(object contextObject)
        {
            if (contextObject is IEnumerable enumerable and not string)
            {
                return ProcessIEumerable(enumerable);
            }
            else
            {
                object result = UnpackSelectExpandWrapper(contextObject);
                if (result is IDictionary<string, object> dict)
                {
                    ProcessDictionary(dict);
                }

                return result;
            }
        }

        static dynamic[] ProcessIEumerable(IEnumerable enumerable)
        {
            dynamic[] rawDataItems = enumerable
                .ToDynamicArray()
                .Select(i => UnpackSelectExpandWrapper(i))
                .ToArray();

            rawDataItems.ForEach(r =>
            {
                if (r is IDictionary<string, object> dict)
                {
                    ProcessDictionary(dict);
                }
            });

            return rawDataItems;
        }

        static object UnpackSelectExpandWrapper(object contextObject)
            => (contextObject is ISelectExpandWrapper wrapper)
                    ? wrapper.ToDictionary().ToObject()
                    : contextObject;

        static void ProcessDictionary(IDictionary<string, object> dict)
        {
            string[] keys = dict.Keys.ToArray();
            foreach (string key in keys)
            {
                dict[key] = HandleOData(dict[key]);
            }
        }
    }
}