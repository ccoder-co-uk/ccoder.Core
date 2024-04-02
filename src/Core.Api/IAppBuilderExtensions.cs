using Microsoft.AspNetCore.Diagnostics;
using System.Security;

namespace Core.Api
{
    public static class IAppBuilderExtensions
    {
        internal static IApplicationBuilder UseCoreFormatters(this IApplicationBuilder app) => 
            app.Use((context, next) =>
            {
                Dictionary<string, Microsoft.Extensions.Primitives.StringValues> query = 
                    Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(context.Request.QueryString.Value);

                if (query.ContainsKey("t"))
                    context.Request.Headers["Authorization"] = $"bearer {query["t"][0]}";

                if (query.TryGetValue("$format", out Microsoft.Extensions.Primitives.StringValues value))
                {
                    context.Request.Headers["Accept"] = value[0] switch
                    {
                        "xml" => "application/xml",
                        "csv" => "text/csv",
                        "excel" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        _ => context.Request.Headers["Content-Type"]
                    };

                    context.Response.Headers["Content-Disposition"] = query["$format"][0] switch
                    {
                        "xml" => "attachment; filename=export.xml",
                        "csv" => "attachment; filename=export.csv",
                        "excel" => "attachment; filename=export.xlsx",
                        _ => "attachment; filename=export.json"
                    };
                }

                return next();
            });

        public static IApplicationBuilder HandleExceptions(this IApplicationBuilder app) =>
            app.UseExceptionHandler(errorApp => errorApp.Run(async (context) =>
            {
                var log = context.RequestServices.GetService<ILogger<IApplicationBuilder>>();
                Exception ex = context.Features.Get<IExceptionHandlerPathFeature>()?.Error;

                context.Response.StatusCode = ex?.GetType() == typeof(SecurityException) 
                    ? 401 
                    : 500;

                context.Response.ContentType = "application/json";

                if (ex != null)
                {
                    log.LogError(message: ex.Message + "\n" + ex.StackTrace);
                    await context.Response.WriteAsync("{ \"error\": \"" + ex.Message.Replace("\"", "\'") + "\" }");

                    Exception innerEx = ex.InnerException;

                    while (innerEx != null)
                    {
                        log.LogError(message: ex.Message + "\n" + ex.StackTrace);
                        innerEx = innerEx.InnerException;
                    }
                }
            }));
    }
}