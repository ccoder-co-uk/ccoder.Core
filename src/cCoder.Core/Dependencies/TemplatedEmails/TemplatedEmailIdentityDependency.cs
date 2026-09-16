// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.AppSecurity.Brokers;
using cCoder.Data.Models.Mail;
using cCoder.Data.Models.Security;
using cCoder.Mail.Exposures;

namespace cCoder.Core.Dependencies.TemplatedEmails;

internal sealed class TemplatedEmailIdentityDependency(
    IAuthorizationBroker authorizationBroker,
    IMailSenderManager mailSenderManager)
    : IAuthorizationBroker
{
    public User GetCurrentUser() =>
        authorizationBroker.GetCurrentUser();

    public bool IsAdminOfApp(int? appId) =>
        authorizationBroker.IsAdminOfApp(appId: appId);

    public bool IsAdmin(int appId, string userName) =>
        authorizationBroker.IsAdmin(
            appId: appId,
            userName: userName);

    public void Authorize(int? appId, string privilege) =>
        authorizationBroker.Authorize(
            appId: appId,
            privilege: privilege);

    internal IQueryable<MailSender> GetAllMailSender(bool ignoreFilters) =>
        mailSenderManager.GetAllMailSender(ignoreFilters: ignoreFilters);
}