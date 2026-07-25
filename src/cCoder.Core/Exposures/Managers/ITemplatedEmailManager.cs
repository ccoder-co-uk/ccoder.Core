// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using CoreApp = cCoder.Data.Models.CMS.App;
using QueuedEmail = cCoder.Data.Models.Mail.QueuedEmail;
using TemplatedEmailDetails = cCoder.Mail.Models.TemplatedEmailDetails;

namespace cCoder.Core.Exposures.Managers;

internal interface ITemplatedEmailManager
{
    ValueTask<QueuedEmail> QueueAppTemplatedEmailAsync(
        CoreApp app,
        string templateName,
        string culture,
        object model,
        string toEmail,
        string subject,
        string sentByUserId,
        string mailSenderName = "Default");

    ValueTask<QueuedEmail> QueueTemplatedEmailDetailsAsync(
        TemplatedEmailDetails details);
}