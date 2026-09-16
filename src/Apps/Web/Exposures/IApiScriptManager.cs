// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Web.Models;
using System.IO;

namespace Web.Exposures;

public interface IApiScriptManager
{
    ValueTask<string> ExecuteApiScriptRequestAsync(
        ApiScriptRequest apiScriptRequest);

    ValueTask<string> ReadRequestBodyAsync(Stream requestBody);
    void LogError(Exception exception);
}