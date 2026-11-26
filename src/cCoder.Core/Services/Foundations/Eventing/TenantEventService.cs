// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Linq;
using System.Threading.Tasks;
using cCoder.Core.Brokers.ContentManagement;
using cCoder.Eventing.Models.Exceptions;

namespace cCoder.Core.Services.Foundations.Eventing;

internal sealed partial class TenantEventService(IContentManagementAppBroker appBroker)
    : ITenantEventService
{
    public ValueTask ValidateDeleteAsync(string tenantId) =>
        TryCatch(operation: () =>
        {
            ValidateTenantId(tenantId: tenantId);

            if (appBroker
                .GetAllApps(ignoreFilters: true)
                .Any(predicate: app => app.TenantId == tenantId))
            {
                throw new LocalEventRejectedException(code: "linked_apps");
            }

            return ValueTask.CompletedTask;
        });
}