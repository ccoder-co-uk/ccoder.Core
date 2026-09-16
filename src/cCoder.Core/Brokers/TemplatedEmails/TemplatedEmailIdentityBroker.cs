// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Dependencies.TemplatedEmails;
using cCoder.Data.Models.Mail;
using cCoder.Data.Models.Security;

namespace cCoder.Core.Brokers.TemplatedEmails;

internal sealed class TemplatedEmailIdentityBroker(
    TemplatedEmailIdentityDependency templatedEmailIdentityDependency)
    : ITemplatedEmailIdentityBroker
{
    public User GetCurrentUser() =>
        templatedEmailIdentityDependency.GetCurrentUser();

    public IQueryable<MailSender> GetAllMailSender(bool ignoreFilters) =>
        templatedEmailIdentityDependency.GetAllMailSender(
            ignoreFilters: ignoreFilters);
}