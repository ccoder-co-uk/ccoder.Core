using Newtonsoft.Json;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Xsl;

namespace Core.Objects.Workflow.Activities.Transformation
{
    public class XmlXslActivity<TResult> : TransformationActivity<string, TResult>
    {

        public string Xslt { get; set; }


        [JsonIgnore]
        public dynamic[] FlattenedResult => Data.Flatten(Result);

        public override async Task Execute()
        {
            if (Source == null || Source.Length < 2)
            {
                Log(Dtos.Workflow.WorkflowLogLevel.Warning, "   Data source appears to be empty, nothing to transform.");
                return;
            }

            if (Xslt != null && Xslt.Length > 2)
            {
                // build the transform
                XslCompiledTransform t = new();
                t.Load(new XmlTextReader(new StringReader(Xslt)));

                using XmlTextReader input = new(new MemoryStream(Encoding.UTF8.GetBytes(Source)));
                using XmlTextWriter output = new(new MemoryStream(), Encoding.UTF8);
                t.Transform(input, output);
                _ = output.BaseStream.Seek(0, SeekOrigin.Begin);
                using StreamReader reader = new(output.BaseStream);
                Result = Data.ParseXml<TResult>(await reader.ReadToEndAsync());
            }
            else
            {
                Log(Dtos.Workflow.WorkflowLogLevel.Warning, "   No Xsl tranform has been provided, to run on the data.");
            }
        }
    }
}