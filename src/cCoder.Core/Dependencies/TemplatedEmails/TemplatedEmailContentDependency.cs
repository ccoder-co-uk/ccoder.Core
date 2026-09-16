// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.ContentManagement.Exposures;
using cCoder.Data.Models.CMS;

namespace cCoder.Core.Dependencies.TemplatedEmails;

internal sealed class TemplatedEmailContentDependency(
    IAppManager appManager,
    ITemplateRenderer templateRenderer)
    : ITemplateRenderer
{
    public string Render(
        int appId,
        string name,
        string culture,
        dynamic model) =>
        templateRenderer.Render(
            appId: appId,
            name: name,
            culture: culture,
            model: model);

    internal App GetAppByDomain(string domain) =>
        appManager.GetByDomain(
            domain: domain,
            ignoreFilters: true);
}