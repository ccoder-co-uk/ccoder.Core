// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data.Models;
using System.IO;

namespace Web.Services.Foundations.Api;

internal interface IApiContextService
{
    ApiInfo[] GetApiInfos();
    ValueTask<string> ReadRequestBodyAsync(Stream requestBody);
}