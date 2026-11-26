// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Threading.Tasks;
using cCoder.Core.Services.Foundations.Eventing;
using cCoder.Eventing;

namespace cCoder.Core.Events;

internal sealed class TenantAppDeletingHandler(ITenantEventService tenantEventService)
    : ILocalEventHandler<string>
{
    public ValueTask HandleAsync(string data) =>
        tenantEventService.ValidateDeleteAsync(tenantId: data);
}