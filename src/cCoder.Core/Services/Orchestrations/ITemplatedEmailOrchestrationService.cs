// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Threading.Tasks;
using QueuedEmail = cCoder.Data.Models.Mail.QueuedEmail;
using TemplatedEmailDetails = cCoder.Mail.Models.TemplatedEmailDetails;

namespace cCoder.Core.Services.Orchestrations;

public interface ITemplatedEmailOrchestrationService
{
    ValueTask<QueuedEmail> QueueTemplatedEmailDetailsAsync(
        TemplatedEmailDetails templatedEmailDetails);
}