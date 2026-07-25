// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.Core.Services.Processings.Setup;

internal interface IBaselineAssetCatalogProcessingService
{
    string LoadDefaultAppConfig();

    byte[] LoadAssetBytes(string relativePath);

    string[] LoadDmsAssetPaths();

}