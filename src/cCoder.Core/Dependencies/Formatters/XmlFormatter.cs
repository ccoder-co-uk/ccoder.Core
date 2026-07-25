// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Text;
using cCoder.Core.Services.Processings.Formatters;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;


namespace cCoder.Core.Dependencies.Formatters;

public class XmlFormatter : TextOutputFormatter
{
    private readonly IFormatterODataProcessingService formatterODataProcessingService;

    public XmlFormatter()
        : this(new FormatterODataProcessingService())
    {
    }

    internal XmlFormatter(
        IFormatterODataProcessingService formatterODataProcessingService)
    {
        this.formatterODataProcessingService = formatterODataProcessingService;
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

    private StringBuilder GetBuffer(
        OutputFormatterWriteContext context,
        Encoding selectedEncoding
    )
    {
        if (selectedEncoding == null)
        {
            throw new ArgumentNullException(nameof(selectedEncoding));
        }

        string json = JsonConvert.SerializeObject(
value: new { item = formatterODataProcessingService.HandleOData(contextObject: context.Object) }
        );

        System.Xml.Linq.XDocument xml = JsonConvert.DeserializeXNode(value: json, deserializeRootElementName: "root");
        return new StringBuilder(xml.ToString());
    }
}