// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.AppSecurity.Brokers;

namespace Web.Services.Foundations.Api;

internal sealed partial class ApiScriptAuthorizationService(
    IAuthorizationBroker authorizationBroker)
    : IApiScriptAuthorizationService
{
    public void AuthorizeScriptExecution() =>
        TryCatch(operation: () =>
        {
            ValidateAuthorizationOnExecute();

            authorizationBroker.Authorize(
                appId: (int?)null,
                privilege: "script_execute");
        });
}