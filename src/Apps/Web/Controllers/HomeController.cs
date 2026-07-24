// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Dynamic;
using cCoder.ContentManagement.Exposures;
using cCoder.ContentManagement.Models;
using cCoder.ContentManagement.Services.Processings;
using cCoder.Data;
using cCoder.Core.Models;
using cCoder.Core.Services.Setup;
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

        IAppProcessingService AppProcessingService { get; }
        IPageRenderer PageRenderer { get; }
        IFirstTimeSetupStateService SetupStateService { get; }

        ICoreAuthInfo AuthInfo =>
            HttpContext?.RequestServices.GetService<ICoreAuthInfo>()
            ?? new CoreAuthInfo { SSOUserId = "Guest" };

        string Host => Request.Host.Host.Replace(oldValue: "www.",newValue: "")
                .ToLowerInvariant();

        dynamic DynamicSessionObject
        {
            get
            {
                dynamic result = new ExpandoObject();
                IDictionary<string, object> values = (IDictionary<string, object>)result;

                result.apiRoot = (Request.Host.Port is not 443 and not 80)
                    ? $"{Request.Scheme}://{Host}:{Request.Host.Port}/Api/"
                    : $"{Request.Scheme}://{Host}/Api/";

                if (!string.IsNullOrWhiteSpace(value: AuthInfo.SSOUserId)
                    && !string.Equals(a: AuthInfo.SSOUserId,b: "Guest",comparisonType: StringComparison.OrdinalIgnoreCase))
                {
                    values["user"] = AuthInfo.SSOUserId;
                }

                string token = Request.Query["t"].ToString();
                if (!string.IsNullOrWhiteSpace(value: token))
                {
                    values["token"] = token;
                }

                if (!CanUseSession())
                {
                    return result;
                }

                foreach (string i in HttpContext.Session.Keys)
                {
                    if (i == "ssoUser")
                    {
                        values["user"] = AuthInfo.SSOUserId;
                    }
                    else
                    {
                        values[i] = GetSessionValue(key: i);
                    }
                }

                return result;
            }
        }

        public HomeController(
            IAppProcessingService appService,
            IPageRenderer pageRenderer,
            IFirstTimeSetupStateService setupStateService,
            ILogger<HomeController> log)
        {
            AppProcessingService = appService;
            PageRenderer = pageRenderer;
            SetupStateService = setupStateService;
            this.log = log;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string path = null,
            string theme = null,
            string culture = null,
            bool edit = false,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!await SetupStateService.IsInitializedAsync(cancellationToken: cancellationToken))
                {
                    return View(
viewName:                         "~/Views/Setup/Index.cshtml",model:                         new FirstTimeSetupViewModel
                        {
                            Domain = SetupRequestHostNormalizer.Normalize(host: Request.Host.Host),
                        });
                }

                if (path?.ToLower()
                    .EndsWith(value: ".php") ?? false)
                {
                    Response.HttpContext.Abort();
                    return Ok();
                }

                if (path?.ToLower() == "robots.txt")
                {
                    return Content(content: "User-agent: * Allow: *",contentType: "text/plain");
                }

                path ??= string.Empty;

                culture = Response.HttpContext.Request.Query.ContainsKey(key: "culture")
                    ? Response.HttpContext.Request.Query["culture"].ToString()
                    : null;

                if (culture != null)
                {
                    SetSessionValue(key: "culture",value: culture);
                }
                else
                {
                    culture = GetSessionValue(key: "culture") ?? string.Empty;
                }

                if (theme != null)
                {
                    SetSessionValue(key: "theme",value: theme);
                }
                else
                {
                    theme = GetSessionValue(key: "theme") ?? string.Empty;
                }

                PageRenderResponse response = PageRenderer.Render(
request:                     new PageRenderRequest
                    {
                        Host = Host,
                        Path = path,
                        Theme = theme,
                        Culture = culture,
                        Edit = edit,
                        RequestUrl = Request.GetEncodedUrl()
                    });

                SetSessionValue(key: "theme",value: response.Theme);
                SetSessionValue(key: "culture",value: response.Culture);

                RenderResult page = response.Page;

                SetupViewBag(edit: edit,app: response.App,page: page);

                ViewResult viewResult = View(model: page);
                viewResult.StatusCode = page.StatusCode;
                return viewResult;
            }
            catch (Exception ex)
            {
                return Error(ex: ex);
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

            session.page = new
            {
                page.AppId,
                page.PageId,
                page.ParentId,
            };

            ViewData["Session"] = session;
            ViewData["Edit"] = edit;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            try
            {
                if (!await SetupStateService.IsInitializedAsync(cancellationToken: context.HttpContext.RequestAborted))
                {
                    await base.OnActionExecutionAsync(context: context,next: next);
                    return;
                }

                App app = AppProcessingService
                    .GetAllApp(ignoreFilters: true)
                    .Where(predicate: a => a.Domain == Host)
                    .Select(selector: a => new App
                    {
                        Id = a.Id,
                        Domain = a.Domain,
                        DefaultCultureId = a.DefaultCultureId,
                        DefaultTheme = a.DefaultTheme
                    })
                    .FirstOrDefault();

                if (app != null && GetSessionValue(key: "theme") == null)
                {
                    SetSessionValue(key: "theme",value: app.DefaultTheme ?? "Default");
                }

                if (app != null && GetSessionValue(key: "culture") == null)
                {
                    SetSessionValue(key: "culture",value: app.DefaultCultureId ?? string.Empty);
                }
            }
            catch (Exception ex)
            {
                log.LogWarning(message: $"Unable to determine the current app domain and set defaults for request on {Request.Host.Host}\n{ex.Message}\n{ex.StackTrace}");
            }

            await base.OnActionExecutionAsync(context: context,next: next);
        }

        IActionResult Error(Exception ex)
        {
            log.LogWarning(message: $"Problem with page request: {ex.Message}\n{ex.StackTrace}");
            log.LogWarning(message: $"   Source: {Request.HttpContext.Connection.RemoteIpAddress}:{Request.HttpContext.Connection.RemotePort}");

            try
            {
                string errorPageQuery = $"Core/Page/Render()?host={Host}&path=Error&theme={GetSessionValue(key: "theme")}&culture={GetSessionValue(key: "culture")}";
                log.LogInformation(message: $"GET {errorPageQuery}");

                PageRenderResponse response = PageRenderer.RenderError(
request:                     new PageRenderRequest
                    {
                        Host = Host,
                        Theme = GetSessionValue(key: "theme"),
                        Culture = GetSessionValue(key: "culture"),
                        RequestUrl = Request.GetEncodedUrl(),
                        Exception = ex
                    });

                return View(viewName: "Index",model: response.Page);
            }
            catch { return PartialView(viewName: "Error",model: ex); }
        }

        string GetSessionValue(string key) =>
            CanUseSession() && HttpContext.Session.Keys.Contains(value: key.ToLowerInvariant())
                ? HttpContext.Session.GetString(key: key)
                : null;

        void SetSessionValue(string key, string value)
        {
            if (!CanUseSession())
            {
                return;
            }

            if (value != null)
            {
                HttpContext.Session.SetString(key: key.ToLowerInvariant(),value: value);
            }
            else if (HttpContext.Session.Keys.Contains(value: key.ToLowerInvariant()))
            {
                HttpContext.Session.Remove(key: key.ToLowerInvariant());
            }
        }

        bool CanUseSession()
        {
            try
            {
                return HttpContext.Session?.IsAvailable == true;
            }
            catch
            {
                return false;
            }
        }
    }
}