using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Xsl;

namespace cCoder.Core.Objects.Workflow.Activities.Transformation
{
    public class XslActivity : TransformationActivity<string, string>
    {

        public string Xslt { get; set; }

        public override async Task Execute()
        {
            // build the transform
            XslCompiledTransform t = new();
            t.Load(new XmlTextReader(new StringReader(Xslt)));

            using XmlTextReader input = new(new MemoryStream(Encoding.UTF8.GetBytes(Source)));
            using XmlTextWriter output = new(new MemoryStream(), Encoding.UTF8);
            t.Transform(input, output);
            _ = output.BaseStream.Seek(0, SeekOrigin.Begin);
            using StreamReader reader = new(output.BaseStream);
            Result = await reader.ReadToEndAsync();
        }
    }
}