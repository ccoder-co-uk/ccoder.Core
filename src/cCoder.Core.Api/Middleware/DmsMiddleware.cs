using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.CMS;
using cCoder.Core.Services;
using System.Security;

namespace cCoder.Core.Api.Middleware;

public class DMSMiddleware
{
    private readonly ILogger log;

    public DMSMiddleware(RequestDelegate next, ILogger<DMSMiddleware> log)
    {
        ArgumentNullException.ThrowIfNull(next);
        this.log = log;
    }

    public async Task InvokeAsync(HttpContext context, ICoreDataContext ctx)
    {
        string path = context.Request.Path.Value[(context.Request.Path.Value.IndexOf("/dms/", StringComparison.CurrentCultureIgnoreCase) + 5)..];
        App app = ctx.GetAll<App>(false).FirstOrDefault(r => r.Domain == context.Request.Host.Host);
        Dictionary<string, Microsoft.Extensions.Primitives.StringValues> query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(context.Request.QueryString.Value);

        var eventService = context.RequestServices.GetService<IEventService>();

        DMSInstance dms = new(app, ctx, eventService, log);

        try
        {
            await HandleRequest(context, path, app, query, dms);
        }
        catch (SecurityException ex)
        {
            log.LogError($"An unhandled exception occurred whilst processing a DMS request to app on domain {app?.Domain ?? "Unknown"} ...");
            log.LogError($"Request details - Path: {path}, Query: {context.Request.QueryString.Value}");
            log.LogError(ex.Message);

            context.Response.Headers.Append("Access-Control-Allow-Origin", context.Request.Host.Host);
            context.Response.Headers.Append("Access-Control-Allow-Headers", "access-control-allow-origin,authorization,content-type,x-requested-with");
            context.Response.Headers.Append("Access-Control-Allow-Methods", "PUT,POST,GET,DELETE,OPTIONS");
            context.Response.Headers.Append("Cache-Control", "max-age=2592000");
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

    private async Task HandleRequest(HttpContext context, string path, App app, Dictionary<string, Microsoft.Extensions.Primitives.StringValues> query, DMSInstance dms)
    {
        bool download = query.ContainsKey("download");
        switch (context.Request.Method)
        {
            case "OPTIONS":
                await Respond(context, null, "application/json");
                break;
            case "GET":
                log.LogInformation($"DMS({app.Id}) Get {path}");

                string search = query.TryGetValue("search", out Microsoft.Extensions.Primitives.StringValues searchValue)
                    ? searchValue[0]
                    : string.Empty;

                int version = query.TryGetValue("version", out Microsoft.Extensions.Primitives.StringValues versionValue)
                    ? int.Parse(versionValue[0])
                    : 0;

                string[] downloadPaths = query.TryGetValue("downloadPaths", out Microsoft.Extensions.Primitives.StringValues pathsValue)
                    ? pathsValue[0].Split(",")
                    : [];

                DMSResult result;

                if (downloadPaths.Length > 0)
                {
                    Objects.Path[] paths = downloadPaths.Select(v => new Objects.Path(v)).ToArray();
                    result = dms.GetFilesZipped(paths);
                }
                else
                    result = dms.Get(new Objects.Path(path), version, search);

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
                await dms.Drop(
                    new Objects.Path(path),
                    query.TryGetValue("version", out Microsoft.Extensions.Primitives.StringValues value)
                        ? int.Parse(value[0])
                        : 0);

                await Respond(context, null, "application/json");
                break;
        }
    }

    private async Task HandlePutRequest(HttpContext context, string path, Dictionary<string, Microsoft.Extensions.Primitives.StringValues> query, DMSInstance dms)
    {
        using MemoryStream memoryStream = new();
        await context.Request.Body.CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        if (query.ContainsKey("moveTo"))
        {
            if (query.TryGetValue("moveTo", out Microsoft.Extensions.Primitives.StringValues newPath))
                await dms.Move(new Objects.Path(path.Split("?")[0]), new Objects.Path(newPath.ToArray()[0]));

            await Respond(context, null, "application/json");
        }
        else
        {
            Objects.Path destinationPath = new(path);

            if (query.ContainsKey("unpack"))
                if (destinationPath.IsToFile)
                {
                    log.LogError($"User request to unpack an archive to a file path failed, The path is: {path}");
                    throw new InvalidOperationException("Cannot unpack an archive to a file path");
                }
                else
                {
                    bool ignoreArchiveRoot = query.ContainsKey("ignoreArchiveRoot") && query["ignoreArchiveRoot"] == "true";
                    await dms.Unpack(destinationPath, memoryStream, ignoreArchiveRoot);
                }
            else
                await dms.Save(destinationPath, memoryStream);

            await Respond(context, null, "application/json");
        }
    }

    protected async Task Respond(HttpContext context, Stream with, string contentType)
    {
        context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
        context.Response.Headers.Append("Access-Control-Allow-Headers", "access-control-allow-origin,authorization,content-type,x-requested-with");
        context.Response.Headers.Append("Access-Control-Allow-Methods", "PUT,POST,GET,DELETE,OPTIONS");
        context.Response.Headers.Append("Cache-Control", "max-age=2592000");
        context.Response.ContentType = contentType;
        context.Response.StatusCode = with != null ? 200 : 204;

        if (with != null)
        {
            context.Response.Headers.Append("Content-Length", with.Length.ToString());
            await with.CopyToAsync(context.Response.Body);
            with.Close();
        }
        context.Response.Body.Close();
    }
}