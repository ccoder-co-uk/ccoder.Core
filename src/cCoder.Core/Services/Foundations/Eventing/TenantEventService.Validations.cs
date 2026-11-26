// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System;

namespace cCoder.Core.Services.Foundations.Eventing;

internal sealed partial class TenantEventService
{
    private static void ValidateTenantId(string tenantId) =>
        ArgumentException.ThrowIfNullOrWhiteSpace(argument: tenantId);
}