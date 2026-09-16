// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data.Models.CMS;

namespace cCoder.Core.Brokers.Eventing;

internal interface IAppGraphEventBroker
{
    ValueTask RaiseAppAddEventAsync(App app);

    ValueTask RaiseAppUpdateEventAsync(App app);
}