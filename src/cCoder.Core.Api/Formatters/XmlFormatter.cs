using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using System.Text;

namespace cCoder.Core.Api.Formatters;

public class XmlFormatter : TextOutputFormatter
{
    public XmlFormatter()
    {
        SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/xml"));
        SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("text/xml"));

        SupportedEncodings.Add(Encoding.UTF8);
        SupportedEncodings.Add(Encoding.Unicode);
    }

    protected override bool CanWriteType(Type type) => true;

    public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
    {
        StringBuilder buffer = GetBuffer(context, selectedEncoding);
        await context.HttpContext.Response.WriteAsync(buffer.ToString());
    }

    private static StringBuilder GetBuffer(OutputFormatterWriteContext context, Encoding selectedEncoding)
    {
        if (selectedEncoding == null)
        {
            throw new ArgumentNullException(nameof(selectedEncoding));
        }

        string json = JsonConvert.SerializeObject(new { item = FormatterODataHelper.HandleOData(context.Object) });
        System.Xml.Linq.XDocument xml = JsonConvert.DeserializeXNode(json, "root");
        return new StringBuilder(xml.ToString());
    }
}