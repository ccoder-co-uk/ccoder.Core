using System.Dynamic;
using cCoder.ContentManagement.Exposures;
using cCoder.ContentManagement.Services.Processings;
using cCoder.Core.Api;
using cCoder.Data;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using App = cCoder.Data.Models.CMS.App;
using RenderResult = cCoder.ContentManagement.Models.RenderResult;


namespace Web.Controllers
{
    public sealed class HomeController : Controller
    {
        readonly ILogger log;

        ICoreAuthInfo AuthInfo { get; }
        IAppProcessingService AppProcessingService { get; }
        IPageRenderer PageRenderer { get; }

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
                    if (i == "ssoUser")
                        ((IDictionary<string, object>)result).Add("user", AuthInfo.SSOUserId);
                    else
                        ((IDictionary<string, object>)result).Add(i, GetSessionValue(i));
                }

                return result;
            }
        }

        public HomeController(IAppProcessingService appService, IPageRenderer pageRenderer, ICoreAuthInfo authInfo, ILogger<HomeController> log)
        {
            AuthInfo = authInfo;
            AppProcessingService = appService;
            PageRenderer = pageRenderer;
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

                PageRenderResponse response = PageRenderer.Render(
                    new PageRenderRequest
                    {
                        Host = Host,
                        Path = path,
                        Theme = theme,
                        Culture = culture,
                        Edit = edit,
                        RequestUrl = Request.GetEncodedUrl()
                    });

                SetSessionValue("theme", response.Theme);
                SetSessionValue("culture", response.Culture);

                RenderResult page = response.Page;

                SetupViewBag(edit, response.App, page);

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
            dynamic session = DynamicSessionObject;

            session.app = new
            {
                app.Id,
                app.TenantId,
                app.Domain,
                app.DefaultCultureId,
                app.DefaultTheme,
                app.Config
            };

            session.page = page.KeyInfo();

            ViewData["Session"] = session;
            ViewData["Edit"] = edit;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            try
            {
                App app = AppProcessingService
                    .GetAll(ignoreFilters: true)
                    .Where(a => a.Domain == Host)
                    .Select(a => new App
                    {
                        Id = a.Id,
                        Domain = a.Domain,
                        DefaultCultureId = a.DefaultCultureId,
                        DefaultTheme = a.DefaultTheme
                    })
                    .FirstOrDefault();

                if (app != null && GetSessionValue("theme") == null)
                    SetSessionValue("theme", app.DefaultTheme ?? "Default");

                if (app != null && GetSessionValue("culture") == null)
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
                string errorPageQuery = $"Core/Page/Render()?host={Host}&path=Error&theme={GetSessionValue("theme")}&culture={GetSessionValue("culture")}";
                log.LogInformation($"GET {errorPageQuery}");

                PageRenderResponse response = PageRenderer.RenderError(
                    new PageRenderRequest
                    {
                        Host = Host,
                        Theme = GetSessionValue("theme"),
                        Culture = GetSessionValue("culture"),
                        RequestUrl = Request.GetEncodedUrl(),
                        Exception = ex
                    });

                return View("Index", response.Page);
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
