// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data.Models.Mail;
using cCoder.Data.Models.Security;

namespace cCoder.Core.Brokers.TemplatedEmails;

internal interface ITemplatedEmailIdentityBroker
{
    User GetCurrentUser();

    IQueryable<MailSender> GetAllMailSender(bool ignoreFilters);
}