// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data.Models.CMS;

namespace cCoder.Core.Brokers.TemplatedEmails;

internal interface ITemplatedEmailContentBroker
{
    App GetAppByDomain(string domain);

    string Render(int appId, string name, string culture, dynamic model);
}