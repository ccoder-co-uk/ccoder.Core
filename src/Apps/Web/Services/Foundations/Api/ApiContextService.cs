// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data.Models;
using Web.Brokers.Api;

namespace Web.Services.Foundations.Api;

internal sealed partial class ApiContextService(
    IApiContextBroker apiContextBroker)
    : IApiContextService
{
    public ApiInfo[] GetApiInfos() =>
        TryCatch(operation: () =>
        {
            ValidateApiInfosOnGet();

            return apiContextBroker.SelectAllApiInfos()
                .Where(predicate: context =>
                    string.Equals(
                        a: context.Kind,
                        b: "Context",
                        comparisonType:
                            StringComparison.OrdinalIgnoreCase))
                .OrderBy(
                    keySelector: context => context.Name,
                    comparer:
                        StringComparer.OrdinalIgnoreCase)
                .ToArray();
        });
}