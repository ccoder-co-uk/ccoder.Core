using cCoder.Core.Services;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Dynamic;

namespace Web.Controllers;

public sealed class HomeController : Controller
{
    private readonly ILogger log;

    private IAppService AppService { get; }
    private IPageService PageService { get; }

    private string Host => 
        Request.Host.Host.Replace("www.", "").ToLowerInvariant();

    private dynamic DynamicSessionObject
    {
        get
        {
            dynamic result = new ExpandoObject();

            result.apiRoot = (Request.Host.Port is not 443 and not 80) 
                ? $"{Request.Scheme}://{Host}:{Request.Host.Port}/Api/" 
                : $"{Request.Scheme}://{Host}/Api/";

            foreach (string i in HttpContext.Session.Keys)
                ((IDictionary<string, object>)result).Add(i, GetSessionValue(i));

            return result;
        }
    }

    public HomeController(IAppService appService, IPageService pageService, ILogger<HomeController> log)
    {
        AppService = appService;
        PageService = pageService;
        this.log = log;
    }

    [HttpGet]
    public IActionResult Index(string path = null, string theme = null, string culture = null, bool edit = false)
    {
        try
        {
            if (path?.ToLower().EndsWith(".php") ?? false)
            {
                Response.HttpContext.Abort();
                return Ok();
            }

            if (path?.ToLower() == "robots.txt")
                return Content("User-agent: * Allow: *", "text/plain");

            cCoder.Core.Objects.Entities.CMS.App app = AppService.GetAll().First(a => a.Domain == Host);

            if (theme != null)
                SetSessionValue("theme", theme);

            if (culture != null)
                SetSessionValue("culture", culture);

            if (app.Id == 0)
                throw new InvalidOperationException("Domain Not found!");

            cCoder.Core.Objects.Dtos.RenderResult page = PageService.Render(app.Id, path, theme ?? GetSessionValue("theme").ToString(), culture ?? GetSessionValue("culture").ToString(), edit);

            ViewBag.Session = DynamicSessionObject;
            ViewBag.Session.app = new { app.Id, app.Domain, app.DefaultCultureId, app.DefaultTheme, app.Config, app.Cultures };
            ViewBag.Session.page = page.KeyInfo();
            ViewBag.Edit = edit;

            ViewResult viewResult = View(page);
            viewResult.StatusCode = page.StatusCode;
            return viewResult;
        }
        catch (Exception ex) { return Error(ex); }
    }

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        try
        {
            cCoder.Core.Objects.Entities.CMS.App app = AppService.GetAll().First(a => a.Domain == Host);

            if (GetSessionValue("theme") == null)
                SetSessionValue("theme", app.DefaultTheme ?? "Default");

            if (GetSessionValue("culture") == null)
                SetSessionValue("culture", app.DefaultCultureId ?? string.Empty);
        }
        catch (Exception ex) { log.LogWarning($"Unable to determine the current app domain and set defaults for request on {Request.Host.Host}\n{ex.Message}\n{ex.StackTrace}"); }
        await base.OnActionExecutionAsync(context, next);
    }

    private IActionResult Error(Exception ex)
    {
        log.LogWarning($"Problem with page request: {ex.Message}\n{ex.StackTrace}");
        log.LogWarning($"   Source: {Request.HttpContext.Connection.RemoteIpAddress}:{Request.HttpContext.Connection.RemotePort}");

        // attempt to recover the apps own custom error page, or provide the system default defined below
        try
        {
            cCoder.Core.Objects.Entities.CMS.App app = AppService.GetAll().FirstOrDefault(a => a.Domain == Host);
            string errorPageQuery = $"Core/Page/Render()?appId={app.Id}&path=Error&theme={GetSessionValue("theme")}&culture={GetSessionValue("culture")}";
            
            if (app.Id > 0) 
                log.LogInformation($"GET {errorPageQuery}"); 

            cCoder.Core.Objects.Dtos.RenderResult page = app.Id > 0
                ? PageService.Render(app.Id, "Error", GetSessionValue("theme").ToString(), GetSessionValue("culture").ToString())
                : throw new Exception("Unknown Domain");

            page.BodyHtml = page.BodyHtml.Replace("[problem[message]]", ex.Message);
            page.BodyHtml = page.BodyHtml.Replace("[problem[detail]]", ex.StackTrace);
            page.BodyHtml = page.BodyHtml.Replace("[problem[url]]", Request.GetEncodedUrl());

            return View("Index", page);
        }
        catch 
        { 
            return PartialView("Error", ex); 
        }
    }

    private string GetSessionValue(string key) => 
        HttpContext.Session.Keys.Contains(key.ToLowerInvariant())
            ? HttpContext.Session.GetString(key)
            : null;

    private void SetSessionValue(string key, string value)
    {
        if (value != null)
            HttpContext.Session.SetString(key.ToLowerInvariant(), value);
        else if (HttpContext.Session.Keys.Contains(key.ToLowerInvariant()))
            HttpContext.Session.Remove(key.ToLowerInvariant());
    }
}