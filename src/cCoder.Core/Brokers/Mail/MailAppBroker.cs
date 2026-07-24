// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Mail.Exposures;
using cCoder.Mail.Models;
using cCoder.Data.Models.CMS;
using cCoder.Data.Models.Mail;
using cCoder.Data.Models.Security;

namespace cCoder.Core.Brokers.Mail;

internal class MailAppBroker(IMailAppExposure mailAppExposure) : IMailAppBroker
{
    public ValueTask AddAppAsync(App newApp) =>
        mailAppExposure.AddAsync(newApp: newApp);

    public ValueTask UpdateAppAsync(App updatedApp) =>
        mailAppExposure.UpdateAsync(updatedApp: updatedApp);

    public ValueTask DeleteAsync(int appId) =>
        mailAppExposure.DeleteAsync(appId: appId);
}