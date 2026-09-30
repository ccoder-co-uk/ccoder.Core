// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System;
using System.Threading.Tasks;
using cCoder.Data.Models;
using System.IO;

namespace Web.Services.Foundations.Api;

public interface IApiContextService
{
    ApiInfo[] GetApiInfos();
    ValueTask<string> ReadRequestBodyAsync(Stream requestBody);
    void LogError(Exception exception);
}