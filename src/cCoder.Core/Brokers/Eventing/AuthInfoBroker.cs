// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data;

namespace cCoder.Core.Brokers.Eventing;

internal sealed class AuthInfoBroker(ICoreAuthInfo authInfo)
    : IAuthInfoBroker
{
    public string GetCurrentSsoUserId() =>
        authInfo.SSOUserId;
}