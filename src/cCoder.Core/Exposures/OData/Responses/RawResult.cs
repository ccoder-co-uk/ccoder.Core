// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;


namespace cCoder.Core.Exposures.OData.Responses;

public class RawResult : ContentResult
{
    public RawResult(string response)
    {
        Content = response;
        StatusCode = StatusCodes.Status200OK;
    }
}