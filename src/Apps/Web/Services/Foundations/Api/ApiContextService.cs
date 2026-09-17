// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data.Models;
using System.IO;
using Web.Brokers.Api;
using Web.Exposures;

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

    public ValueTask<string> ReadRequestBodyAsync(Stream requestBody) =>
        TryCatch(operation: async () =>
        {
            ValidateRequestBodyOnRead(requestBody: requestBody);

            return await apiContextBroker.ReadRequestBodyAsync(
                requestBody: requestBody);
        });

    void IApiContextManager.LogError(Exception exception) =>
        apiContextBroker.LogError(exception: exception);
}