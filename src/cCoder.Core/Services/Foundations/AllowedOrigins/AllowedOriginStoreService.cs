// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Brokers.ContentManagement;
using cCoder.Core.Brokers.Http;
using cCoder.Core.Brokers.Json;
using cCoder.Data.Models.CMS;

namespace cCoder.Core.Services.Foundations.AllowedOrigins;

internal sealed partial class AllowedOriginStoreService(
    IContentManagementAppBroker appBroker,
    IHttpRequestBroker httpRequestBroker,
    IAllowedOriginJsonBroker allowedOriginJsonBroker)
    : IAllowedOriginStoreService
{
    public ValueTask<string[]> GetAllowedOriginsAsync() =>
        TryCatch(operation: () =>
        {
            ValidateAllowedOriginsOnGet();

            HttpRequest request = httpRequestBroker.GetCurrentRequest();
            string domain = request?.Host.Host;

            if (!string.IsNullOrWhiteSpace(value: domain))
            {
                App app = appBroker.GetAppByDomain(
                    domain: domain,
                    ignoreFilters: true);

                string[] origins = app is null
                    ? []
                    : [.. GetAllowedOrigins(app: app)
                        .Where(predicate: origin => !string.IsNullOrWhiteSpace(value: origin))
                        .Distinct(comparer: StringComparer.OrdinalIgnoreCase)];

                return ValueTask.FromResult(result: origins);
            }

            return ValueTask.FromResult(result: Array.Empty<string>());
        });

    private IEnumerable<string> GetAllowedOrigins(App app)
    {
        if (!string.IsNullOrWhiteSpace(value: app.Domain))
        {
            yield return app.Domain;
        }

        foreach (string origin in allowedOriginJsonBroker.ExtractOrigins(configJson: app.ConfigJson))
        {
            yield return origin;
        }
    }

}