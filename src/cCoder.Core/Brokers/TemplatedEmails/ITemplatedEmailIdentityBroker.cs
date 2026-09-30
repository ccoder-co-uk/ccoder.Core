// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Linq;
using cCoder.Data.Models.Mail;
using cCoder.Data.Models.Security;

namespace cCoder.Core.Brokers.TemplatedEmails;

internal interface ITemplatedEmailIdentityBroker
{
    User GetCurrentUser();

    IQueryable<MailSender> GetAllMailSenders(bool ignoreFilters);
}
