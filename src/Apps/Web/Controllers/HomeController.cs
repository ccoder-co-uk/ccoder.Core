using cCoder.Core.Api;
using cCoder.Core.Objects.Dtos;
using cCoder.Core.Objects.Entities.CMS;
using cCoder.Core.Services;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Dynamic;

namespace Web.Controllers
{
    public sealed class HomeController : Controller
    {
        readonly ILogger log;

        IAppService AppService { get; }
        IPageService PageService { get; }

        string Host => Request.Host.Host.Replace("www.", "").ToLowerInvariant();

        dynamic DynamicSessionObject
        {
            get
            {
                dynamic result = new ExpandoObject();

                result.apiRoot = (Request.Host.Port is not 443 and not 80) 
                    ? $"{Request.Scheme}://{Host}:{Request.Host.Port}/Api/" 
                    : $"{Request.Scheme}://{Host}/Api/";

                foreach (string i in HttpContext.Session.Keys)
                {
                    if(i == "ssoUser")
                        ((IDictionary<string, object>)result).Add("user", AppService.AuthInfo.SSOUserId);
                    else
                        ((IDictionary<string, object>)result).Add(i, GetSessionValue(i));
                }

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

                if (!HttpContext.Session.IsAvailable)
                    throw new Exception("Cannot load session information");

                culture = Response.HttpContext.Request.Query.ContainsKey("culture")
                    ? Response.HttpContext.GetQueryParameter("culture")
                    : null;

                if (culture != null)
                    SetSessionValue("culture", culture);
                else
                    culture = GetSessionValue("culture") ?? string.Empty;

                if (theme != null)
                    SetSessionValue("theme", theme);
                else
                    theme = GetSessionValue("theme") ?? string.Empty;

                App app = AppService
                    .GetAll()
                    .First(a => a.Domain == Host);

                if (app.Id == 0)
                    throw new InvalidOperationException("Domain Not found!");

                RenderResult page = PageService
                    .Render(app.Id, path, theme, culture, edit);

                SetupViewBag(edit, app, page);

                ViewResult viewResult = View(page);
                viewResult.StatusCode = page.StatusCode;
                return viewResult;
            }
            catch (Exception ex)
            {
                return Error(ex);
            }
        }

        private void SetupViewBag(bool edit, App app, RenderResult page)
        {
            ViewBag.Session = DynamicSessionObject;

            ViewBag.Session.app = new
            {
                app.Id,
                app.TenantId,
                app.Domain,
                app.DefaultCultureId,
                app.DefaultTheme,
                app.Config,
                app.Cultures
            };

            ViewBag.Session.page = page.KeyInfo();
            ViewBag.Edit = edit;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            try
            {
                App app = AppService
                    .GetAll()
                    .First(a => a.Domain == Host);

                if (GetSessionValue("theme") == null)
                    SetSessionValue("theme", app.DefaultTheme ?? "Default");

                if (GetSessionValue("culture") == null)
                    SetSessionValue("culture", app.DefaultCultureId ?? string.Empty);
            }
            catch (Exception ex)
            {
                log.LogWarning($"Unable to determine the current app domain and set defaults for request on {Request.Host.Host}\n{ex.Message}\n{ex.StackTrace}");
            }

            await base.OnActionExecutionAsync(context, next);
        }

        IActionResult Error(Exception ex)
        {
            log.LogWarning($"Problem with page request: {ex.Message}\n{ex.StackTrace}");
            log.LogWarning($"   Source: {Request.HttpContext.Connection.RemoteIpAddress}:{Request.HttpContext.Connection.RemotePort}");

            // attempt to recover the apps own custom error page, or provide the system default defined below
            try
            {
                App app = AppService.GetAll().FirstOrDefault(a => a.Domain == Host);

                if (app == null)
                    throw new Exception("Unknown Domain");

                string errorPageQuery = $"Core/Page/Render()?appId={app.Id}&path=Error&theme={GetSessionValue("theme")}&culture={GetSessionValue("culture")}";
                log.LogInformation($"GET {errorPageQuery}");

                RenderResult page = PageService.Render(app.Id, "Error", GetSessionValue("theme").ToString(), GetSessionValue("culture").ToString());

                page.BodyHtml = page.BodyHtml.Replace("[problem[message]]", ex.Message);
                page.BodyHtml = page.BodyHtml.Replace("[problem[detail]]", ex.StackTrace);
                page.BodyHtml = page.BodyHtml.Replace("[problem[url]]", Request.GetEncodedUrl());

                return View("Index", page);
            }
            catch { return PartialView("Error", ex); }
        }

        string GetSessionValue(string key) => 
            HttpContext.Session.Keys.Contains(key.ToLowerInvariant())
                ? HttpContext.Session.GetString(key)
                : null;

        void SetSessionValue(string key, string value)
        {
            if (value != null)
                HttpContext.Session.SetString(key.ToLowerInvariant(), value);
            else if (HttpContext.Session.Keys.Contains(key.ToLowerInvariant()))
                HttpContext.Session.Remove(key.ToLowerInvariant());
        }
    }
}