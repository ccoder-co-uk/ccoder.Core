// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Security;
using cCoder.ContentManagement.Exposures;
using cCoder.ContentManagement.Models;
using cCoder.ContentManagement.Models.Exceptions;
using cCoder.Data;
using cCoder.Core.Models;
using cCoder.Core.Exposures.Setup;
using cCoder.Core.Exposures;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Web.Exposures;
using App = cCoder.Data.Models.CMS.App;
using RenderResult = cCoder.ContentManagement.Models.RenderResult;
using System.ComponentModel.DataAnnotations;


namespace Web.Controllers
{
    public sealed class HomeController(
        IPageRenderer pageRenderer,
        IFirstTimeSetupManager setupStateService,
        ISetupRequestHostManager setupRequestHostManager,
        IHomeSessionManager homeSessionManager,
        ILogger<HomeController> logger) : Controller
    {
        private readonly IPageRenderer pageRenderer = pageRenderer;
        private readonly IFirstTimeSetupManager setupStateService = setupStateService;
        private readonly ISetupRequestHostManager setupRequestHostManager = setupRequestHostManager;
        private readonly IHomeSessionManager homeSessionManager = homeSessionManager;

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

                PageRenderResponse response = await pageRenderer.RenderAsync();

                homeSessionManager.SetSessionValue(
                    context: HttpContext,
                    key: "theme",
                    value: response.Theme);

                homeSessionManager.SetSessionValue(
                    context: HttpContext,
                    key: "culture",
                    value: response.Culture);

                RenderResult page = response.Page;

                SetupViewBag(
                    edit: response.Edit,
                    app: response.App,
                    page: page);

                ViewResult viewResult = View(model: page);
                viewResult.StatusCode = page.StatusCode;
                return viewResult;
            }
            catch (ValidationException)
            {
                return BadRequest(error: "The page request is invalid.");
            }
            catch (PageAccessSecurityException)
            {
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
            catch (SecurityException)
            {
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
            catch (ContentManagementDependencyException)
            {
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