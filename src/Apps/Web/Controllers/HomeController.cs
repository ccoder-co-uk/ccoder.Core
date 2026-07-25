// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Dynamic;
using cCoder.ContentManagement.Exposures;
using cCoder.ContentManagement.Models;
using cCoder.Data;
using cCoder.Core.Models;
using cCoder.Core.Exposures.Setup;
using cCoder.Core.Services.Setup;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Web.Dependencies.Filters;
using Web.Exposures;
using App = cCoder.Data.Models.CMS.App;
using RenderResult = cCoder.ContentManagement.Models.RenderResult;


namespace Web.Controllers
{
    public sealed class HomeController : Controller
    {
        private readonly ILogger log;

        private readonly IPageRenderer pageRenderer;
        private readonly IFirstTimeSetupStateService setupStateService;
        private readonly ISetupRequestHostManager setupRequestHostManager;
        private readonly IHomeSessionManager homeSessionManager;

        private ICoreAuthInfo GetAuthInfo() =>
            HttpContext?.RequestServices.GetService<ICoreAuthInfo>()
            ?? new CoreAuthInfo { SSOUserId = "Guest" };

        private string GetHost() =>
            Request.Host.Host.Replace(oldValue: "www.",newValue: "")
                .ToLowerInvariant();

        private ExpandoObject CreateExpandoObject()
        {
            dynamic result = new ExpandoObject();
            IDictionary<string, object> values = (IDictionary<string, object>)result;

            result.apiRoot = (Request.Host.Port is not 443 and not 80)
                ? $"{Request.Scheme}://{GetHost()}:{Request.Host.Port}/Api/"
                : $"{Request.Scheme}://{GetHost()}/Api/";

            ICoreAuthInfo authInfo = GetAuthInfo();

            if (!string.IsNullOrWhiteSpace(value: authInfo.SSOUserId)
                && !string.Equals(a: authInfo.SSOUserId,b: "Guest",comparisonType: StringComparison.OrdinalIgnoreCase))
            {
                values["user"] = authInfo.SSOUserId;
            }

            string token = Request.Query["t"].ToString();

            if (!string.IsNullOrWhiteSpace(value: token))
            {
                values["token"] = token;
            }

            if (!homeSessionManager.CanUseSession(
                context: HttpContext))
            {
                return result;
            }

            foreach (string key in HttpContext.Session.Keys)
            {
                if (key == "ssoUser")
                {
                    values["user"] = authInfo.SSOUserId;
                }
                else
                {
                    values[key] =
                        homeSessionManager.GetSessionValue(
                            context: HttpContext,
                            key: key);
                }
            }

            return (ExpandoObject)result;
        }

        public HomeController(
            IPageRenderer pageRenderer,
            IFirstTimeSetupStateService setupStateService,
            ISetupRequestHostManager setupRequestHostManager,
            IHomeSessionManager homeSessionManager,
            ILogger<HomeController> log)
        {
            this.pageRenderer = pageRenderer;
            this.setupStateService = setupStateService;
            this.setupRequestHostManager = setupRequestHostManager;
            this.homeSessionManager = homeSessionManager;
            this.log = log;
        }

        [HttpGet]
        [ServiceFilter(typeof(HomeDefaultsActionFilter))]
        public async Task<IActionResult> Index(
            string path = null,
            string theme = null,
            string culture = null,
            bool edit = false,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!await setupStateService.IsInitializedAsync(cancellationToken: cancellationToken))
                {
                    return View(
viewName:                         "~/Views/Setup/Index.cshtml",model:                         new FirstTimeSetupViewModel
                        {
                            Domain = setupRequestHostManager.NormalizeHost(
                                host: Request.Host.Host),
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
                    homeSessionManager.SetSessionValue(
                        context: HttpContext,
                        key: "culture",
                        value: culture);
                }
                else
                {
                    culture =
                        homeSessionManager.GetSessionValue(
                            context: HttpContext,
                            key: "culture")
                        ?? string.Empty;
                }

                if (theme != null)
                {
                    homeSessionManager.SetSessionValue(
                        context: HttpContext,
                        key: "theme",
                        value: theme);
                }
                else
                {
                    theme =
                        homeSessionManager.GetSessionValue(
                            context: HttpContext,
                            key: "theme")
                        ?? string.Empty;
                }

                PageRenderResponse response = pageRenderer.Render(
request:                     new PageRenderRequest
                    {
                        Host = GetHost(),
                        Path = path,
                        Theme = theme,
                        Culture = culture,
                        Edit = edit,
                        RequestUrl = Request.GetEncodedUrl()
                    });

                homeSessionManager.SetSessionValue(
                    context: HttpContext,
                    key: "theme",
                    value: response.Theme);

                homeSessionManager.SetSessionValue(
                    context: HttpContext,
                    key: "culture",
                    value: response.Culture);

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
            dynamic session = CreateExpandoObject();

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

        IActionResult Error(Exception ex)
        {
            log.LogWarning(message: $"Problem with page request: {ex.Message}\n{ex.StackTrace}");
            log.LogWarning(message: $"   Source: {Request.HttpContext.Connection.RemoteIpAddress}:{Request.HttpContext.Connection.RemotePort}");

            try
            {
                string theme =
                    homeSessionManager.GetSessionValue(
                        context: HttpContext,
                        key: "theme");

                string culture =
                    homeSessionManager.GetSessionValue(
                        context: HttpContext,
                        key: "culture");

                string errorPageQuery =
                    $"Core/Page/Render()?host={GetHost()}&path=Error&theme={theme}&culture={culture}";

                log.LogInformation(message: $"GET {errorPageQuery}");

                PageRenderResponse response = pageRenderer.RenderError(
request:                     new PageRenderRequest
                    {
                        Host = GetHost(),
                        Theme = theme,
                        Culture = culture,
                        RequestUrl = Request.GetEncodedUrl(),
                        Exception = ex
                    });

                return View(viewName: "Index",model: response.Page);
            }
            catch { return PartialView(viewName: "Error",model: ex); }
        }

    }
}