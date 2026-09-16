// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace Web.Brokers.Api;

internal interface IMetadataCacheBroker
{
    void Rebuild();
    string GetAll(string culture);
}