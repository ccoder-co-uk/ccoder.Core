// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.AppSecurity.Models;
using cCoder.Data.Models.CMS;
using cCoder.Data.Models.Security;

namespace cCoder.Core.Brokers.AppSecurity;

public interface IAppSecurityAppBroker
{
    ValueTask AddAppAsync(App newApp);
    ValueTask UpdateAppAsync(App updatedApp);
    ValueTask DeleteAppAsync(int appId);
}