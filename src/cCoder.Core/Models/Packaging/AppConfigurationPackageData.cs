// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.Core.Models.Packaging;

internal sealed class AppConfigurationPackageData
{
    public int Id { get; set; }
    public string DefaultCultureId { get; set; }
    public string TenantId { get; set; }
    public string Name { get; set; }
    public string Domain { get; set; }
    public string DefaultTheme { get; set; }
    public string ConfigJson { get; set; }
}