// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.Core.Brokers.Eventing;

internal interface IAuthInfoBroker
{
    string GetCurrentSsoUserId();
}