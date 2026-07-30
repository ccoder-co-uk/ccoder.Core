// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using CoreApp = cCoder.Data.Models.CMS.App;
using QueuedEmail = cCoder.Data.Models.Mail.QueuedEmail;
using TemplatedEmailDetails = cCoder.Mail.Models.TemplatedEmailDetails;

using cCoder.Core.Exposures.Managers;

namespace cCoder.Core.Services.Orchestrations;

internal interface ITemplatedEmailOrchestrationService : ITemplatedEmailManager { }