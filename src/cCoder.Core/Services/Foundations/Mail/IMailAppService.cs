// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data.Models.CMS;

namespace cCoder.Core.Services.Foundations.Mail;

public interface IMailAppService
{
    ValueTask AddAppAsync(App newApp);
    ValueTask UpdateAppAsync(App updatedApp);
    ValueTask DeleteAppAsync(int appId);
}