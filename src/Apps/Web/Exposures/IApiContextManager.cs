// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data.Models;
using System.IO;

namespace Web.Exposures;

public interface IApiContextManager
{
    ApiInfo[] GetApiInfos();
    ValueTask<string> ReadRequestBodyAsync(Stream requestBody);
}