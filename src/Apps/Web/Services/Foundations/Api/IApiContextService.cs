// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data.Models;

namespace Web.Services.Foundations.Api;

internal interface IApiContextService
{
    ApiInfo[] GetApiInfos();
}