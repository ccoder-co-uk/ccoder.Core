// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Threading.Tasks;
using cCoder.Core.Models;

namespace cCoder.Core.Services.Foundations.TemplatedEmails;

internal interface ITemplatedEmailQueueService
{
    ValueTask<TemplatedEmailOperation> QueueTemplatedEmailOperationAsync(
        TemplatedEmailOperation templatedEmailOperation);
}