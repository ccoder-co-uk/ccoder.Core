// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Threading.Tasks;

namespace cCoder.Core.Services.Foundations.Eventing;

internal interface ITenantEventService
{
    ValueTask ValidateDeleteAsync(string tenantId);
}