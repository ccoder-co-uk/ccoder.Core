using cCoder.Core;
using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.CMS;
using cCoder.Core.Objects.Entities.DMS;
using cCoder.Core.Objects.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Security;
using System.Text;
using System.Xml.Linq;
using File = cCoder.Core.Objects.Entities.DMS.File;
using MemoryStream = System.IO.MemoryStream;
using Path = cCoder.Core.Objects.Path;
using Stream = System.IO.Stream;

namespace Web.Api.Middleware;

public class WebDavMiddleware
{
    readonly ILogger log;

    public WebDavMiddleware(RequestDelegate next, ILogger<WebDavMiddleware> log)
    {
        ArgumentNullException.ThrowIfNull(next);
        this.log = log;
    }

    public async Task InvokeAsync(HttpContext context, ICoreDataContext ctx, Config config)
    {
        int appId = int.Parse(context.Request.Path.Value.TrimStart("Core/App(".ToCharArray()).Split(')')[0]);
        Path path = new(WebUtility.UrlDecode(context.Request.Path.Value.Replace($"Core/App({appId})/DAV", "")).TrimStart('/').TrimEnd("/".ToCharArray()));
        HttpRequest request = context.Request;
        byte[] buffer = new byte[Convert.ToInt32(request.ContentLength)];
        _ = await request.Body.ReadAsync(buffer);
        string requestText = Encoding.UTF8.GetString(buffer);

        log.LogDebug($"HTTP {context.Request.Method.ToUpper()} - {path} \n {requestText}");
        XNamespace ns = "DAV:";
        App app = ctx.Get<App>(appId);

        Dictionary<string, Microsoft.Extensions.Primitives.StringValues> query = 
            Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(context.Request.QueryString.Value);

        DMS dms = new(app, ctx, log);

        string sslPort = config.Settings["sslPort"] ?? "443";
        string urlBase = $"https://{app.Domain}:{sslPort}/Api/";

        List<KeyValuePair<string, string>> headers =
        [
            new KeyValuePair<string, string>("Host", urlBase + $"DAV/")
        ];

        if (!context.Request.Headers.ContainsKey("Authorization"))
        {
            headers.Add(new KeyValuePair<string, string>("WWW-Authenticate", "Basic realm=\"server\""));
            headers.Add(new KeyValuePair<string, string>("Connection", "close"));
            context.Response.StatusCode = 401;
            await Respond(context, "", headers);
            return;
        }

        try
        {
            switch (context.Request.Method.ToUpper())
            {
                case "OPTIONS":
                    headers.AddRange([
                        new KeyValuePair<string, string>("Access-Control-Allow-Origin", "*"),
                        new KeyValuePair<string, string>("Allow", "OPTIONS, GET, HEAD, POST, PUT, DELETE, TRACE, COPY, MOVE, MKCOL, PROPFIND, PROPPATCH, LOCK, UNLOCK, ORDERPATCH"),
                        new KeyValuePair<string, string>("Public", "OPTIONS, GET, HEAD, POST, PUT, DELETE, TRACE, COPY, MOVE, MKCOL, PROPFIND, PROPPATCH, LOCK, UNLOCK, ORDERPATCH"),
                        new KeyValuePair<string, string>("Date", DateTimeOffset.Now.ToString("s") + "Z"),
                        new KeyValuePair<string, string>("DAV", "1, 2"),
                        new KeyValuePair<string, string>("MS-Author-Via", "DAV")
                    ]);

                    await Respond(context, null, headers);
                    break;

                case "GET":
                    int getVer = query.TryGetValue("version", out Microsoft.Extensions.Primitives.StringValues value) 
                        ? int.Parse(value[0]) 
                        : 0;

                    DMSResult result = dms.Get(path, getVer);
                    context.Response.StatusCode = 200;
                    await Respond(context, result.Data, result.MimeType, headers);
                    break;

                case "HEAD":
                    await Respond(context, null, headers);
                    break;

                case "PROPFIND":
                    string body = PropFind(context, ctx, appId, path, requestText, ns, urlBase);
                    context.Response.StatusCode = body != string.Empty ? 207 : 404;
                    await Respond(context, body, headers);
                    break;

                case "PROPPATCH":
                    string responseXmlElement = new XElement(ns + "multistatus", 
                        [
                            new XAttribute(XNamespace.Xmlns + "D", "DAV:"), new XAttribute(XNamespace.Xmlns + "Z", "urn:schemas-microsoft-com:")
                        ]).ToXml();

                    context.Response.StatusCode = 200;
                    await Respond(context, responseXmlElement, headers);
                    break;

                case "POST":
                case "PUT":
                    context.Response.StatusCode = 201;
                    await dms.Save(path, new MemoryStream(buffer));
                    await Respond(context, context.Request.Body, context.Request.Headers.ContentType, headers);
                    break;

                case "MKCOL":
                    await dms.Save(path, context.Request.Body);
                    await Respond(context, null, headers);
                    break;

                case "MOVE":
                    Path moveDest = new(WebUtility.UrlDecode(context.Request.Headers["Destination"]).Split($"Core/App({appId})/DAV")[1]);
                    await dms.Move(path, moveDest);
                    await Respond(context, null, headers);
                    break;

                case "COPY":
                    string copyDest = GetHeaderValue(context, "Destination").Replace($"Core/App({appId})/DMS/", "");
                    await dms.Move(path, new Path(copyDest));
                    await Respond(context, null, headers);
                    break;

                case "DELETE":
                    await dms.Drop(path, query.TryGetValue("version", out Microsoft.Extensions.Primitives.StringValues versionValue) 
                        ? int.Parse(versionValue[0]) 
                        : 0);

                    await Respond(context, null, headers);
                    break;

                case "LOCK":
                    context.Response.StatusCode = 200;
                    await Respond(context, null, headers);
                    break;

                case "UNLOCK":
                    context.Response.StatusCode = 204;
                    await Respond(context, null, headers);
                    break;
            }
        }
        catch (SecurityException)
        {
            headers.AddRange(
            [
                new KeyValuePair<string, string>("WWW-Authenticate", "Basic realm=\"server\"")
            ]);

            await Respond(context, null, headers);
        }
        catch (Exception ex)
        {
            await Respond(context, ex.Message, headers);
        }
    }

    string PropFind(HttpContext context, ICoreDataContext ctx, int appId, Path path, string requestText, XNamespace ns, string urlBase)
    {
        XDocument requestBody = requestText.Length > 0 && context.Request.Headers.ContentType == "application/xml" 
            ? XDocument.Parse(requestText) 
            : new XDocument();

        IEnumerable<string> requestedProperties = requestBody.Descendants(ns + "prop").DescendantNodes().Select(k => ((XElement)k).Name.LocalName);

        return !path.IsToFile
            ? PropFindFolder(context, ctx, appId, path, ns, urlBase, requestedProperties)
            : PropFindFile(ctx, appId, path, ns, urlBase, requestedProperties);
    }

    string PropFindFile(ICoreDataContext ctx, int appId, Path path, XNamespace ns, string urlBase, IEnumerable<string> requestedProperties)
    {
        try
        {
            File file = ctx.GetAll<File>(false)
                .Include(k => k.Contents)
                .Include(k => k.Folder)
                .FirstOrDefault(f => f.Folder.AppId == appId && path.Length > 0 && f.Path.Equals(path.FullPath, StringComparison.CurrentCultureIgnoreCase));

            XElement response = file?.ToWebDavResponse(urlBase, ns, requestedProperties);
            string responsePropXml = new XElement(ns + "multistatus", new object[] { new XAttribute(XNamespace.Xmlns + "D", "DAV:"), new XAttribute(XNamespace.Xmlns + "Z", "urn:schemas-microsoft-com:") }
                    .Union([response]))
                .ToXml();

            return responsePropXml;
        }
        catch (Exception ex)
        {
            log.LogError(ex.Message + "\n" + ex.StackTrace);
            return string.Empty;
        }
    }

    private static string PropFindFolder(HttpContext context, ICoreDataContext ctx, int appId, Path path, XNamespace ns, string urlBase, IEnumerable<string> requestedProperties)
    {
        Folder folder = path.FullPath != "" 
            ? ctx.GetAll<Folder>(false)
                .Include(f => f.SubFolders)
                .Include(f => f.Files)
                .FirstOrDefault(f => f.AppId == appId && f.Path == path.FullPath)
            : new Folder
            {
                Name = "Root",
                SubFolders = [.. ctx.GetAll<Folder>(false).Where(f => f.AppId == appId && f.ParentId == null)],
                Files = [],
                Path = "",
                AppId = appId
            };

        List<Folder> folders = [];
        List<File> files = [];

        if (int.Parse(context.Request.Headers["Depth"]) > 0)
        {
            folders = [.. ctx.GetAll<Folder>(false).Where(f => f.AppId == appId && (path.FullPath != "" 
                ? f.Parent.Path.Equals(path.FullPath, StringComparison.CurrentCultureIgnoreCase) 
                : f.ParentId == null))
                    .Include(k => k.Files)
                    .Include(k => k.SubFolders)
                    .Include(k => k.Parent)];

            files = [.. ctx.GetAll<File>(false).Where(f => f.Folder.AppId == appId && path.Length > 0 && f.Folder.Path == path.FullPath)
                .Include(k => k.Contents)
                .Include(k => k.Folder)];
        }

        if (folder != null)
        {
            folders.Insert(0, folder);
        }

        IEnumerable<XElement> response = folders
            .Select(k => k.ToWebDavResponse(urlBase, ns, requestedProperties))
                .Union(files.Select(k => k.ToWebDavResponse(urlBase, ns, requestedProperties)));

        string responsePropXml = new XElement(ns + "multistatus", new object[] { new XAttribute(XNamespace.Xmlns + "D", "DAV:"), new XAttribute(XNamespace.Xmlns + "Z", "urn:schemas-microsoft-com:") }
            .Union(response))
            .ToXml();

        return responsePropXml;
    }

    static string GetHeaderValue(HttpContext context, string key) => context.Request.Headers[key][0];


    protected async Task Respond(HttpContext context, string content, IEnumerable<KeyValuePair<string, string>> headers)
    {
        headers.ForEach(h =>
        {
            if (!context.Response.Headers.ContainsKey(h.Key))
                context.Response.Headers.Append(h.Key, h.Value);
        });

        context.Response.ContentType = "text/xml; charset=\"utf-8\"";

        if (content != null)
        {
            await using MemoryStream outStream = new(Encoding.UTF8.GetBytes(content));
            context.Response.Headers.Append("Content-Length", content.Length.ToString());
            await outStream.CopyToAsync(context.Response.Body);
            outStream.Close();
        }
        else
            context.Response.StatusCode = 204;
    }

    protected async Task Respond(HttpContext context, Stream with, string contentType, IEnumerable<KeyValuePair<string, string>> headers)
    {
        headers.ForEach(h =>
        {
            if (!context.Response.Headers.ContainsKey(h.Key))
                context.Response.Headers.Append(h.Key, h.Value);
        });

        context.Response.ContentType = contentType;

        if (with != null)
        {
            context.Response.Headers.Append("Content-Length", with.Length.ToString());
            await with.CopyToAsync(context.Response.Body);
            context.Response.Body.Close();
        }
        else
            context.Response.StatusCode = 204;
    }
}