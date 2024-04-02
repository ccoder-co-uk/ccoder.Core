using Core;
using Core.Objects;
using Core.Objects.Entities.CMS;
using System.Security;

namespace Web.Api.Middleware
{
    public class DMSMiddleware
    {
        readonly ILogger log;

        public DMSMiddleware(RequestDelegate next, ILogger<DMSMiddleware> log)
        {
            if (next is null)
                throw new ArgumentNullException(nameof(next));

            this.log = log;
        }

        public async Task InvokeAsync(HttpContext context, ICoreDataContext ctx)
        {
            string path = context.Request.Path.Value[(context.Request.Path.Value.ToLower().IndexOf("/dms/") + 5)..];
            App app = ctx.GetAll<App>(false).FirstOrDefault(r => r.Domain == context.Request.Host.Host);
            Dictionary<string, Microsoft.Extensions.Primitives.StringValues> query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(context.Request.QueryString.Value);
            DMS dms = new(app, ctx, log);

            try
            {
                await HandleRequest(context, path, app, query, dms);
            }
            catch (SecurityException ex)
            {
                log.LogError($"An unhandled exception occurred whilst processing a DMS request to app on domain {app?.Domain ?? "Unknown"} ...");
                log.LogError($"Request details - Path: {path}, Query: {context.Request.QueryString.Value}");
                log.LogError(ex.Message);

                context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                context.Response.Headers.Add("Access-Control-Allow-Headers", "access-control-allow-origin,authorization,content-type,x-requested-with");
                context.Response.Headers.Add("Access-Control-Allow-Methods", "PUT,POST,GET,DELETE,OPTIONS");
                context.Response.Headers.Add("Cache-Control", "max-age=2592000");
                throw;
            }
            catch (Exception ex)
            {
                log.LogError($"An unhandled exception occurred whilst processing a DMS request to app on domain {app?.Domain ?? "Unknown"} ...");
                log.LogError($"Request details - Path: {path}, Query: {context.Request.QueryString.Value}");
                log.LogError(ex.Message);
                throw;
            }
        }

        private async Task HandleRequest(HttpContext context, string path, App app, Dictionary<string, Microsoft.Extensions.Primitives.StringValues> query, DMS dms)
        {
            bool download = query.ContainsKey("download");
            switch (context.Request.Method)
            {
                case "OPTIONS":
                    await Respond(context, null, "application/json");
                    break;
                case "GET":
                    log.LogInformation($"DMS({app.Id}) Get {path}");

                    string search = query.ContainsKey("search")
                        ? query["search"][0]
                        : string.Empty;

                    int version = query.ContainsKey("version") 
                        ? int.Parse(query["version"][0]) 
                        : 0;

                    string[] downloadPaths = query.ContainsKey("downloadPaths") 
                        ? query["downloadPaths"][0].Split(",") 
                        : Array.Empty<string>();

                    DMSResult result;

                    if (downloadPaths.Length > 0)
                    {
                        var paths = downloadPaths.Select(v => new Core.Objects.Path(v)).ToArray();
                        result = dms.GetFilesZipped(paths);
                    }
                    else
                        result = dms.Get(new Core.Objects.Path(path), version, search);

                    if (result != null)
                        await Respond(context, result.Data, download ? "application/octet-stream" : result.MimeType);
                    else
                        await Respond(context, null, "plain/text");

                    break;
                case "POST":
                    log.LogInformation($"DMS({app.Id}) POST {path}");
                    await HandlePutRequest(context, path, query, dms);
                    break;
                case "PUT":
                    log.LogInformation($"DMS({app.Id}) PUT {path}");
                    await HandlePutRequest(context, path, query, dms);
                    break;
                case "DELETE":
                    await dms.Drop(new Core.Objects.Path(path), query.ContainsKey("version") ? int.Parse(query["version"][0]) : 0);
                    await Respond(context, null, "application/json");
                    break;
            }
        }

        private async Task HandlePutRequest(HttpContext context, string path, Dictionary<string, Microsoft.Extensions.Primitives.StringValues> query, DMS dms)
        {
            using var memoryStream = new MemoryStream();
            await context.Request.Body.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            if (query.ContainsKey("moveTo"))
            {
                if (query.TryGetValue("moveTo", out Microsoft.Extensions.Primitives.StringValues newPath))
                    await dms.Move(new Core.Objects.Path(path.Split("?")[0]), new Core.Objects.Path(newPath.ToArray()[0]));

                await Respond(context, null, "application/json");
            }
            else
            {
                var destinationPath = new Core.Objects.Path(path);

                if (query.ContainsKey("unpack"))
                {
                    if (destinationPath.IsToFile)
                    {
                        log.LogError($"User request to unpack an archive to a file path failed, The path is: {path}");
                        throw new InvalidOperationException("Cannot unpack an archive to a file path");
                    }
                    else
                    {
                        var ignoreArchiveRoot = query.ContainsKey("ignoreArchiveRoot") && query["ignoreArchiveRoot"] == "true";
                        await dms.Unpack(destinationPath, memoryStream, ignoreArchiveRoot);
                    }
                }
                else
                    await dms.Save(destinationPath, memoryStream);

                await Respond(context, null, "application/json");
            }
        }

        protected async Task Respond(HttpContext context, Stream with, string contentType)
        {
            context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
            context.Response.Headers.Add("Access-Control-Allow-Headers", "access-control-allow-origin,authorization,content-type,x-requested-with");
            context.Response.Headers.Add("Access-Control-Allow-Methods", "PUT,POST,GET,DELETE,OPTIONS");
            context.Response.Headers.Add("Cache-Control", "max-age=2592000");
            context.Response.ContentType = contentType;
            context.Response.StatusCode = with != null ? 200 : 204;

            if (with != null)
            {
                context.Response.Headers.Add("Content-Length", with.Length.ToString());
                await with.CopyToAsync(context.Response.Body);
                with.Close();
            }
            context.Response.Body.Close();
        }
    }
}