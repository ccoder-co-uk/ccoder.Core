// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data.Models.CMS;

namespace cCoder.Core.Services.Foundations.Eventing;

internal interface IServiceBusAppDeleteForwardingService
{
    ValueTask ForwardAppDeleteAsync(App app);
}