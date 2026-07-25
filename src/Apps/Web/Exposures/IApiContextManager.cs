// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data.Models;

namespace Web.Exposures;

public interface IApiContextManager
{
    ApiInfo[] GetApiInfos();
}