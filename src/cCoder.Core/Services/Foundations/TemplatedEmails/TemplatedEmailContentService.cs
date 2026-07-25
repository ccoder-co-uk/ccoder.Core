// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.ContentManagement.Exposures;
using cCoder.Core.Brokers.ContentManagement;
using cCoder.Core.Models;
using CoreUser = cCoder.Data.Models.Security.User;
using CoreApp = cCoder.Data.Models.CMS.App;

namespace cCoder.Core.Services.Foundations.TemplatedEmails;

internal sealed partial class TemplatedEmailContentService(
    IContentManagementAppBroker contentManagementAppBroker,
    ITemplateRenderer templateRenderer
) : ITemplatedEmailContentService
{
    public TemplatedEmailOperation ResolveTemplatedEmailOperationContent(
        TemplatedEmailOperation templatedEmailOperation) =>
        TryCatch(operation: () =>
        {
            ValidateTemplatedEmailOperationOnResolve(
                templatedEmailOperation: templatedEmailOperation);

            if (templatedEmailOperation.Details is not null)
            {
                templatedEmailOperation.App =
                    contentManagementAppBroker.GetAppByDomain(
                        domain: templatedEmailOperation.Details.SourceDomain,
                        ignoreFilters: true)
                    ?? throw new InvalidOperationException(
                        $"No app found for domain '{templatedEmailOperation.Details.SourceDomain}'");

                templatedEmailOperation.TemplateName =
                    templatedEmailOperation.Details.TemplateName;

                templatedEmailOperation.ToEmail =
                    templatedEmailOperation.Details.ToEmail;
            }

            CoreApp app = templatedEmailOperation.App;

            templatedEmailOperation.Template =
                app.Templates.FirstOrDefault(predicate: candidate =>
                    candidate.Name == templatedEmailOperation.TemplateName)
                ?? throw new InvalidOperationException(
                    $"Template '{templatedEmailOperation.TemplateName}' was not found.");

            return templatedEmailOperation;
        });

    public TemplatedEmailOperation RenderTemplatedEmailOperationContent(
        TemplatedEmailOperation templatedEmailOperation) =>
        TryCatch(operation: () =>
        {
            ValidateTemplatedEmailOperationOnRender(
                templatedEmailOperation: templatedEmailOperation);

            object renderModel = templatedEmailOperation.Model;

            if (templatedEmailOperation.Details is not null)
            {
                CoreUser currentUser = templatedEmailOperation.CurrentUser;
                string culture = templatedEmailOperation.Culture;

                renderModel = new
                {
                    Data = templatedEmailOperation.Details.Model,
                    CoreUser = currentUser is null
                        ? null
                        : new
                        {
                            currentUser.Id,
                            DefaultCultureId = culture,
                            currentUser.DisplayName,
                            currentUser.Email,
                            currentUser.IsActive,
                        },
                };
            }

            templatedEmailOperation.Content = templateRenderer.Render(
                appId: templatedEmailOperation.App.Id,
                name: templatedEmailOperation.Template.Name,
                culture: templatedEmailOperation.Culture,
                model: renderModel);

            return templatedEmailOperation;
        });
}