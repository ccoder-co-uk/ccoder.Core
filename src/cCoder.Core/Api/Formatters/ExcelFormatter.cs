using System.Linq.Dynamic.Core;
using System.Text;
using cCoder.ContentManagement.Exposures.Caching;
using cCoder.Data.Models.CMS;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;


namespace cCoder.Core.Api.Formatters;

public class ExcelFormatter : TextOutputFormatter
{
    public ExcelFormatter()
    {
        SupportedMediaTypes.Add(
            MediaTypeHeaderValue.Parse(
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            )
        );
        SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("text/vnd.ms-excel"));
        SupportedEncodings.Add(Encoding.UTF8);
    }

    protected override bool CanWriteType(Type type) => true;

    public override async Task WriteResponseBodyAsync(
        OutputFormatterWriteContext context,
        Encoding selectedEncoding
    )
    {
        string culture = GetCulture(context, selectedEncoding);
        await FormatterODataHelper
            .HandleOData(context.Object)
            .ToExcel(GetResources(context), culture)
            .CopyToAsync(context.HttpContext.Response.Body);
        await context.HttpContext.Response.Body.FlushAsync();
        context.HttpContext.Response.Body.Close();
    }

    private static string GetCulture(OutputFormatterWriteContext context, Encoding selectedEncoding)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        if (selectedEncoding == null)
            throw new ArgumentNullException(nameof(selectedEncoding));

        return context.HttpContext.Request.Query.ContainsKey("culture")
            ? Thread.CurrentThread.CurrentCulture.Name
            : context.HttpContext.GetQueryParameter("culture");
    }

    public override void WriteResponseHeaders(OutputFormatterWriteContext context)
    {
        base.WriteResponseHeaders(context);
        context.HttpContext.Response.Headers["Content-Disposition"] =
            "Content-Disposition: attachment; Data.xlsx;";
    }

    private static IEnumerable<Resource> GetResources(
        OutputFormatterWriteContext context
    )
    {
        var commonObjectCache = context.HttpContext.RequestServices.GetRequiredService<ICommonObjectCache>();
        Resource[] cachedResources = commonObjectCache.GetAll<Resource>();
        List<Resource> resources = [];
        if (context.HttpContext.Request.Query.ContainsKey("appId"))
        {
            resources.AddRange(
                cachedResources
                    .Where(r =>
                        r.AppId == int.Parse(context.HttpContext.GetQueryParameter("appId"))
                        && r.Key == "Default"
                    )
            );
        }

        resources.AddRange(
            new Resource[]
            {
                new()
                {
                    Name = "dateformat",
                    DisplayName = context.HttpContext.Request.Query.ContainsKey("dateFormat")
                        ? context.HttpContext.GetQueryParameter("dateFormat")
                        : "yyyy-MM-dd",
                },
                new()
                {
                    Name = "moneyformat",
                    DisplayName = context.HttpContext.Request.Query.ContainsKey("moneyFormat")
                        ? context.HttpContext.GetQueryParameter("moneyFormat")
                        : "n",
                },
            }
        );
        resources.AddRange(cachedResources);
        return resources.ToArray();
    }
}






