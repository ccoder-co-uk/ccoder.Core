// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Models;

namespace cCoder.Core.Services.Foundations.TemplatedEmails;

internal interface ITemplatedEmailContentService
{
    TemplatedEmailOperation ResolveTemplatedEmailOperationContent(
        TemplatedEmailOperation templatedEmailOperation);

    TemplatedEmailOperation RenderTemplatedEmailOperationContent(
        TemplatedEmailOperation templatedEmailOperation);
}