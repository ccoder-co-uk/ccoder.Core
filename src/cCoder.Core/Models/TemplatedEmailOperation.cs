// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data.Models.Mail;
using cCoder.Mail.Models;
using CoreUser = cCoder.Data.Models.Security.User;
using CoreApp = cCoder.Data.Models.CMS.App;
using ContentTemplate = cCoder.Data.Models.CMS.Template;
using TemplatedEmailDetails = cCoder.Mail.Models.TemplatedEmailDetails;

namespace cCoder.Core.Models;

internal sealed class TemplatedEmailOperation
{
    public CoreApp App { get; set; }

    public TemplatedEmailDetails Details { get; set; }

    public string TemplateName { get; set; }

    public ContentTemplate Template { get; set; }

    public string Culture { get; set; }

    public object Model { get; set; }

    public string ToEmail { get; set; }

    public string Subject { get; set; }

    public string SentByUserId { get; set; }

    public string MailSenderName { get; set; }

    public MailSender MailSender { get; set; }

    public CoreUser CurrentUser { get; set; }

    public string Content { get; set; }

    public QueuedEmail Email { get; set; }
}