// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data.Models;

namespace cCoder.Core.Services.Processings.Setup;

internal interface IBaselineCommonObjectCatalogProcessingService
{
    CommonObject[] LoadCommonObjects();
}