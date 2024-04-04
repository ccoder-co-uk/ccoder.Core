using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cCoder.Core.Objects.Workflow.Activities.Transformation
{
    public class ConvertActivity<TSource, TResult> : TransformationActivity<IEnumerable<TSource>, TResult[]>
    {

        public IEnumerable<string> Expressions { get; set; }

        public class ScriptArgs<T>
        {
            public IEnumerable<T> Source { get; set; }
        }

        public override async Task Execute() =>
            Result = (await ExecuteScript<IEnumerable<TResult>>(BuildFunctionCode(), new ScriptArgs<TSource> { Source = Source })).ToArray();

        private string BuildFunctionCode()
        {
            string assigns = string.Join(",\n\t\t\t", Expressions?.ToArray() ?? System.Array.Empty<string>()).Replace("{source}", "item").Replace("\n", "\n\t\t");
            return @"   Source.Select((" + typeof(TSource).Name == "object" ? "dynamic" : typeof(TSource).Name + @" item) => {
        return new " + typeof(TResult).Name + @"() 
        { 
            " + assigns + @" 
        };
    })";
        }
    }
}