namespace cCoder.Core.Models;

public class CoreDomainsConfig
{
    public string RootPath { get; set; } = "Api";
    public bool SplitDomains { get; set; }
    public bool IncludeLegacyCoreContext { get; set; }
    public string Connection { get; set; } = string.Empty;
}
