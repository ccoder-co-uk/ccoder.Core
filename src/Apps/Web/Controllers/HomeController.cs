// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.ContentManagement.Exposures;
using cCoder.ContentManagement.Models;
using cCoder.Data;
using cCoder.Core.Models;
using cCoder.Core.Exposures.Setup;
using cCoder.Core.Exposures;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Web.Dependencies.Filters;
using Web.Exposures;
using App = cCoder.Data.Models.CMS.App;
using RenderResult = cCoder.ContentManagement.Models.RenderResult;


namespace Web.Controllers
{
    public sealed class HomeController(
        IPageRenderer pageRenderer,
        IFirstTimeSetupManager setupStateService,
        ISetupRequestHostManager setupRequestHostManager,
        IHomeSessionManager homeSessionManager) : Controller
    {
        private readonly IPageRenderer pageRenderer = pageRenderer;
        private readonly IFirstTimeSetupManager setupStateService = setupStateService;
        private readonly ISetupRequestHostManager setupRequestHostManager = setupRequestHostManager;
        private readonly IHomeSessionManager homeSessionManager = homeSessionManager;

        private string GetHost() =>
            Request.Host.Host.Replace(oldValue: "www.",newValue: "")
                .ToLowerInvariant();

        [HttpGet]
        [ServiceFilter(typeof(HomeDefaultsActionFilter))]
        [ServiceFilter(typeof(HomeExceptionFilter))]
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
            catch (Exception exception) when (
                exception.GetType().Name.Contains(
                    value: "Validation",
                    comparisonType: StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(error: "The page request is invalid.");
            }
            catch (Exception)
            {
                return StatusCode(
                    statusCode: StatusCodes.Status500InternalServerError,
                    value: "The page could not be rendered.");
            }
        }

        private void SetupViewBag(bool edit, App app, RenderResult page)
        {
            dynamic session =
                homeSessionManager.CreateExpandoObject(
                    context: HttpContext);

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

    }
}