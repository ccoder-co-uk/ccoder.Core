using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.CMS;
using cCoder.Core.Objects.Extensions;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using System.Linq.Dynamic.Core;
using System.Text;

namespace cCoder.Core.Api.Formatters
{
    public class CsvFormatter : TextOutputFormatter
    {
        public CsvFormatter()
        {
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/csv"));
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("text/csv"));
            SupportedEncodings.Add(Encoding.UTF8);
        }

        protected override bool CanWriteType(Type type) => true;

        public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            (string delimiter, string quotes, string culture) = ExtractValues(context);
            await context.HttpContext.Response.WriteAsync(FormatterODataHelper.HandleOData(context.Object).ToCsv(GetResources(context, culture), delimiter, quotes, culture));
        }

        static (string delimiter, string quotes, string culture) ExtractValues(OutputFormatterWriteContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            return (
                context.HttpContext.Request.Query.ContainsKey("delimiter") ? context.HttpContext.GetQueryParameter("delimiter") : ", ",
                context.HttpContext.Request.Query.ContainsKey("quotes") ? context.HttpContext.GetQueryParameter("quotes") : "",
                context.HttpContext.Request.Query.ContainsKey("culture") ? context.HttpContext.GetQueryParameter("culture") : "en-GB"
            );
        }

        static IEnumerable<Resource> GetResources(OutputFormatterWriteContext context, string culture)
        {
            ICoreDataContext db = (ICoreDataContext)context.HttpContext.RequestServices.GetService(typeof(ICoreDataContext));
            ICommonObjectCache commonObjectCache = (ICommonObjectCache)context.HttpContext.RequestServices.GetService(typeof(ICommonObjectCache));
            List<Resource> resources = new();
            if (context.HttpContext.Request.Query.ContainsKey("appId"))
            {
                resources.AddRange(db.GetAll<Resource>(false).Where(r => r.AppId == int.Parse(context.HttpContext.GetQueryParameter("appId")) && r.Key == "Default" && r.Culture == culture));
            }

            resources.AddRange(new Resource[]
            {
                new() { Name = "dateformat", DisplayName = context.HttpContext.Request.Query.ContainsKey("dateFormat") ? context.HttpContext.GetQueryParameter("dateFormat") : "yyyy-MM-dd" },
                new() { Name = "moneyformat", DisplayName = context.HttpContext.Request.Query.ContainsKey("moneyFormat") ? context.HttpContext.GetQueryParameter("moneyFormat") : "n" },
            });
            resources.AddRange(commonObjectCache.GetAll<Resource>());
            return resources;
        }
    }
}