// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Models;

namespace cCoder.Core.Services.Foundations.TemplatedEmails;

internal interface ITemplatedEmailIdentityService
{
    TemplatedEmailOperation ResolveTemplatedEmailOperationIdentity(
        TemplatedEmailOperation templatedEmailOperation);
}