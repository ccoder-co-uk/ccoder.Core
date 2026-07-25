// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Microsoft.EntityFrameworkCore;

namespace cCoder.Core.Brokers.Setup;

internal interface ISecuritySetupContextBroker
{
    DbContext CreateSecurityContext();
}