// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System;
using System.Linq;
using System.Threading.Tasks;
using cCoder.Core.Brokers.AllowedOrigins;

namespace cCoder.Core.Services.Foundations.AllowedOrigins;

internal sealed partial class AllowedOriginStoreService(
    IAllowedOriginStoreBroker allowedOriginStoreBroker)
    : IAllowedOriginStoreService
{
    public ValueTask<string[]> GetAllowedOriginsAsync() =>
        TryCatch(operation: () =>
        {
            ValidateAllowedOriginsOnGet();

            string[] origins = [.. allowedOriginStoreBroker.GetAllowedOrigins()
                .Where(predicate: origin => !string.IsNullOrWhiteSpace(value: origin))
                .Distinct(comparer: StringComparer.OrdinalIgnoreCase)];

            return ValueTask.FromResult(result: origins);
        });
}