// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Dependencies.TemplatedEmails;
using cCoder.Data.Models.CMS;

namespace cCoder.Core.Brokers.TemplatedEmails;

internal sealed class TemplatedEmailContentBroker(
    TemplatedEmailContentDependency templatedEmailContentDependency)
    : ITemplatedEmailContentBroker
{
    public App GetAppByDomain(string domain) =>
        templatedEmailContentDependency.GetAppByDomain(domain: domain);

    public string Render(
        int appId,
        string name,
        string culture,
        dynamic model) =>
        templatedEmailContentDependency.Render(
            appId: appId,
            name: name,
            culture: culture,
            model: model);
}