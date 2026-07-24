// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Text;
using cCoder.Core.Exposures.Formatters;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;


namespace cCoder.Core.Dependencies.Formatters;

public class XmlFormatter : TextOutputFormatter
{
    public XmlFormatter()
    {
        SupportedMediaTypes.Add(item: MediaTypeHeaderValue.Parse(input: "application/xml"));
        SupportedMediaTypes.Add(item: MediaTypeHeaderValue.Parse(input: "text/xml"));

        SupportedEncodings.Add(item: Encoding.UTF8);
        SupportedEncodings.Add(item: Encoding.Unicode);
    }

    protected override bool CanWriteType(Type type) =>
        true;

    public override async Task WriteResponseBodyAsync(
        OutputFormatterWriteContext context,
        Encoding selectedEncoding
    )
    {
        StringBuilder buffer = GetBuffer(context: context, selectedEncoding: selectedEncoding);
        await context.HttpContext.Response.WriteAsync(text: buffer.ToString());
    }

    private static StringBuilder GetBuffer(
        OutputFormatterWriteContext context,
        Encoding selectedEncoding
    )
    {
        if (selectedEncoding == null)
        {
            throw new ArgumentNullException(nameof(selectedEncoding));
        }

        string json = JsonConvert.SerializeObject(
value: new { item = FormatterODataHelper.HandleOData(contextObject: context.Object) }
        );

        System.Xml.Linq.XDocument xml = JsonConvert.DeserializeXNode(value: json, deserializeRootElementName: "root");
        return new StringBuilder(xml.ToString());
    }
}
