// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Security;
using cCoder.ContentManagement.Exposures;
using cCoder.ContentManagement.Models;
using cCoder.ContentManagement.Models.Exceptions;
using cCoder.Data;
using cCoder.Core.Models;
using cCoder.Core.Brokers.Loggings;
using cCoder.Core.Exposures.Setup;
using cCoder.Core.Exposures;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Web.Exposures;
using App = cCoder.Data.Models.CMS.App;
using PageRenderResult = cCoder.ContentManagement.Models.PageRenderResult;
using System.ComponentModel.DataAnnotations;


namespace Web.Controllers
{
    public sealed class HomeController(
        IPageRenderer pageRenderer,
        IFirstTimeSetupManager setupStateService,
        ISetupRequestHostManager setupRequestHostManager,
        IHomeSessionManager homeSessionManager,
        ILoggingBroker logger) : Controller
    {
        private readonly IPageRenderer pageRenderer = pageRenderer;
        private readonly IFirstTimeSetupManager setupStateService = setupStateService;
        private readonly ISetupRequestHostManager setupRequestHostManager = setupRequestHostManager;
        private readonly IHomeSessionManager homeSessionManager = homeSessionManager;

        private const string CultureExplicitSessionKey = "cultureexplicit";

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
                if (path?.ToLower()
                    .EndsWith(value: ".php") ?? false)
                {
                    Response.HttpContext.Abort();
                    return Ok();
                }

                if (path?.ToLower() == "robots.txt")
                {
                    return Content(content: "User-agent: * Allow: *", contentType: "text/plain");
                }

                if (!string.IsNullOrWhiteSpace(value: culture))
                {
                    homeSessionManager.SetSessionValue(
                        context: HttpContext,
                        key: CultureExplicitSessionKey,
                        value: bool.TrueString);
                }

                PageRenderResponse response = await pageRenderer.RenderAsync();

                homeSessionManager.SetSessionValue(
                    context: HttpContext,
                    key: "theme",
                    value: response.Theme);

                homeSessionManager.SetSessionValue(
                    context: HttpContext,
                    key: "culture",
                    value: response.Culture);

                PageRenderResult page = response.Page;

                SetupViewBag(
                    edit: response.Edit,
                    app: response.App,
                    page: page);

                ViewResult viewResult = View(model: page);
                viewResult.StatusCode = page.StatusCode;
                return viewResult;
            }
            catch (ValidationException exception)
            {
                logger.LogError(exception: exception, message: "Page request validation failed.");

                return BadRequest(error: "The page request is invalid.");
            }
            catch (PageAccessSecurityException exception)
            {
                logger.LogWarning(exception: exception, message: "Page access was denied.");

                string returnUrl = Request.PathBase + Request.Path
                    + Request.QueryString;

                if (!Url.IsLocalUrl(url: returnUrl))
                {
                    returnUrl = "/";
                }

                return RedirectToAction(
                    actionName: nameof(Index),
                    routeValues: new
                    {
                        path = "Login",
                        returnUrl
                    });
            }
            catch (PageNotFoundException exception)
            {
                logger.LogWarning(
                    exception: exception,
                    message: "Page render request was not found for {Path}.",
                    args: [Request.Path]);

                if (!await setupStateService.IsInitializedAsync(
                    cancellationToken: cancellationToken))
                {
                    return View(
                        viewName: "~/Views/Setup/Index.cshtml",
                        model: new FirstTimeSetupViewModel
                        {
                            Domain = setupRequestHostManager.NormalizeHost(
                                host: Request.Host.Host),
                        });
                }

                return NotFound(value: "The requested page was not found.");
            }
            catch (SecurityException exception)
            {
                logger.LogWarning(exception: exception, message: "Page security processing failed.");

                if (!await setupStateService.IsInitializedAsync(
                    cancellationToken: cancellationToken))
                {
                    return View(
                        viewName: "~/Views/Setup/Index.cshtml",
                        model: new FirstTimeSetupViewModel
                        {
                            Domain = setupRequestHostManager.NormalizeHost(
                                host: Request.Host.Host),
                        });
                }

                throw;
            }
            catch (ContentManagementDependencyException exception)
            {
                logger.LogError(exception: exception, message: "Page content dependency failed.");

                if (!await setupStateService.IsInitializedAsync(
                    cancellationToken: cancellationToken))
                {
                    return View(
                        viewName: "~/Views/Setup/Index.cshtml",
                        model: new FirstTimeSetupViewModel
                        {
                            Domain = setupRequestHostManager.NormalizeHost(
                                host: Request.Host.Host),
                        });
                }

                throw;
            }
            catch (Exception exception)
            {
                logger.LogError(exception: exception, message: "Page rendering failed.");

                return StatusCode(
                    statusCode: StatusCodes.Status500InternalServerError,
                    value: "The page could not be rendered.");
            }
        }

        private void SetupViewBag(bool edit, App app, PageRenderResult page)
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