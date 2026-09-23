// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Text.Json;
using cCoder.Core.Models.Packaging;
using cCoder.CodeAnalysis.Exposures;

namespace cCoder.Core.Brokers.Json;

internal sealed class CorePackageJsonBroker : ICorePackageJsonBroker, IUtilityBroker
{
    public string Serialize(AppConfigurationPackageData appConfigurationPackageData) =>
        JsonSerializer.Serialize(value: appConfigurationPackageData);

    public AppConfigurationPackageData Deserialize(string data) =>
        JsonSerializer.Deserialize<AppConfigurationPackageData>(json: data);
}