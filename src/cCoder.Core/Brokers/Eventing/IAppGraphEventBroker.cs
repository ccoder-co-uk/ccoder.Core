// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data.Models.CMS;
using cCoder.Eventing.Models;

namespace cCoder.Core.Brokers.Eventing;

internal interface IAppGraphEventBroker
{
    ValueTask RaiseAppAddEventAsync(EventMessage<App> message);

    ValueTask RaiseAppUpdateEventAsync(EventMessage<App> message);
}