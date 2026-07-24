// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.Core.Models.Packaging;

internal sealed class AppConfigurationPackageItem
{
    public int Id { get; init; }
    public string DefaultCultureId { get; init; }
    public string TenantId { get; init; }
    public string Name { get; init; }
    public string Domain { get; init; }
    public string DefaultTheme { get; init; }
    public string ConfigJson { get; init; }
}
