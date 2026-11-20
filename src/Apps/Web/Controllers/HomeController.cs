// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Threading.Tasks;
using cCoder.ContentManagement.Exposures;
using cCoder.ContentManagement.Models;
using cCoder.ContentManagement.Models.Exceptions;
using cCoder.Core.Models;
using cCoder.Core.Exposures.Setup;
using cCoder.Core.Exposures;
using Microsoft.AspNetCore.Mvc;
using Web.Services.Processings;
using App = cCoder.Data.Models.CMS.App;
using PageRenderResult = cCoder.ContentManagement.Models.PageRenderResult;
using System.ComponentModel.DataAnnotations;
using System.Security;
using System.Security.Authentication;
using cCoder.Core.Brokers.Loggings;


namespace Web.Controllers
{
    public sealed class HomeController(
        IPageRenderer pageRenderer,
        IFirstTimeSetupManager setupStateService,
        ISetupRequestHostManager setupRequestHostManager,
        IHomeSessionProcessingService homeSessionProcessingService,
        ILoggingBroker loggingBroker) : Controller
    {
        private readonly IPageRenderer pageRenderer = pageRenderer;
        private readonly IFirstTimeSetupManager setupStateService = setupStateService;
        private readonly ISetupRequestHostManager setupRequestHostManager = setupRequestHostManager;
        private readonly IHomeSessionProcessingService homeSessionProcessingService =
            homeSessionProcessingService;

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
                    homeSessionProcessingService.AbortRequest(
                        context: Response.HttpContext);

                    loggingBroker.LogInformation(
                        message: "The scanner request was handled successfully.");


                    return Ok();
                }

                if (path?.ToLower() == "robots.txt")
                {
                    loggingBroker.LogInformation(
                        message: "The robots request was handled successfully.");


                    return Content(content: "User-agent: * Allow: *", contentType: "text/plain");
                }

                if (!string.IsNullOrWhiteSpace(value: culture))
                {
                    homeSessionProcessingService.SetSessionValue(
                        context: HttpContext,
                        key: CultureExplicitSessionKey,
                        value: bool.TrueString);
                }

                PageRenderResponse response = await pageRenderer.RenderAsync();

                homeSessionProcessingService.SetSessionValue(
                    context: HttpContext,
                    key: "theme",
                    value: response.Theme);

                homeSessionProcessingService.SetSessionValue(
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

                loggingBroker.LogInformation(
                    message: "The page was rendered successfully.");


                return viewResult;
            }
            catch (ValidationException exception)
            {
                loggingBroker.LogWarning(
                    exception: exception,
                    message: "The page request is invalid.");

                return BadRequest(error: "The page request is invalid.");
            }
            catch (AuthenticationException exception)
            {
                loggingBroker.LogWarning(
                    exception: exception,
                    message: "The page request was not authenticated.");

                return Unauthorized(value: "Authentication failed.");
            }
            catch (PageAccessSecurityException exception)
            {
                loggingBroker.LogWarning(
                    exception: exception,
                    message: "The page request requires authentication.");

                string returnUrl = Request.PathBase + Request.Path
                    + Request.QueryString;

                if (!homeSessionProcessingService.IsLocalUrl(
                    urlHelper: Url,
                    url: returnUrl))
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
            catch (SecurityException exception)
            {
                loggingBroker.LogWarning(
                    exception: exception,
                    message: "The page request was forbidden.");

                return StatusCode(
                    statusCode: StatusCodes.Status403Forbidden,
                    value: "The page request is forbidden.");
            }
            catch (PageNotFoundException exception)
            {
                if (!await setupStateService.IsInitializedAsync(
                    cancellationToken: cancellationToken))
                {
                    loggingBroker.LogInformation(
                        message: "The setup page was rendered successfully.");


                    return View(
                        viewName: "~/Views/Setup/Index.cshtml",
                        model: new FirstTimeSetupViewModel
                        {
                            Domain = setupRequestHostManager.NormalizeHost(
                                host: Request.Host.Host),
                        });
                }

                loggingBroker.LogWarning(
                    exception: exception,
                    message: "The requested page was not found.");


                return NotFound(value: "The requested page was not found.");
            }
            catch (Exception exception)
            {
                loggingBroker.LogError(
                    exception: exception,
                    message: "The page could not be rendered.");

                return StatusCode(
                    statusCode: StatusCodes.Status500InternalServerError,
                    value: "The page could not be rendered.");
            }
        }

        private void SetupViewBag(bool edit, App app, PageRenderResult page)
        {
            dynamic session =
                homeSessionProcessingService.CreateExpandoObject(
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