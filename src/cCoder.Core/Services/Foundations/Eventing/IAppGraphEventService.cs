// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data.Models.CMS;

namespace cCoder.Core.Services.Foundations.Eventing;

internal interface IAppGraphEventService
{
    ValueTask RaiseAppAddEventAsync(App app);

    ValueTask RaiseAppUpdateEventAsync(App app);
}