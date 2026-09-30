// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Threading.Tasks;
using cCoder.Mail.Exposures;
using cCoder.Data.Models.CMS;

namespace cCoder.Core.Brokers.Mail;

internal class MailAppBroker(IMailAppExposure mailAppExposure) : IMailAppBroker
{
    public ValueTask AddAppAsync(App newApp) =>
        mailAppExposure.AddAsync(newApp: newApp);

    public ValueTask UpdateAppAsync(App updatedApp) =>
        mailAppExposure.UpdateAsync(updatedApp: updatedApp);

    public ValueTask DeleteAppAsync(int appId) =>
        mailAppExposure.DeleteAsync(appId: appId);
}