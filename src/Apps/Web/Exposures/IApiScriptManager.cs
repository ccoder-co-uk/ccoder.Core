// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Web.Models;

namespace Web.Exposures;

public interface IApiScriptManager
{
    ValueTask<string> ExecuteApiScriptRequestAsync(
        ApiScriptRequest request);
}