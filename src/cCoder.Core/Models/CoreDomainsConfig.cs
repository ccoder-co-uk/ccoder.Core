// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.Core.Models;

public class CoreDomainsConfig
{
    public CoreDomainsConfig()
    {
        RootPath = "Api";
        IncludeLegacyCoreContext = true;
        Connection = string.Empty;
    }

    public string RootPath { get; set; }
    public bool SplitDomains { get; set; }
    public bool IncludeLegacyCoreContext { get; set; }
    public string Connection { get; set; }
}