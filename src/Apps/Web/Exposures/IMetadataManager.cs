// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace Web.Exposures;

public interface IMetadataManager
{
    string GetAll(string culture);
    void LogError(Exception exception);
}