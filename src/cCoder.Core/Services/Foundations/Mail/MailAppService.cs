// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Brokers.Mail;
using cCoder.Data.Models.CMS;

namespace cCoder.Core.Services.Foundations.Mail;

internal class MailAppService(IMailAppBroker mailAppBroker) : IMailAppService
{
    public ValueTask AddAsync(App app) =>
        mailAppBroker.AddAsync(app: app);
    public ValueTask UpdateAsync(App app) =>
        mailAppBroker.UpdateAsync(app: app);
    public ValueTask DeleteAsync(int appId) =>
        mailAppBroker.DeleteAsync(appId: appId);
}