// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Models.Packaging;
using cCoder.CodeAnalysis.Exposures;

namespace cCoder.Core.Brokers.Json;

internal interface ICorePackageJsonBroker : IUtilityBroker
{
    string Serialize(AppConfigurationPackageData appConfigurationPackageData);
    AppConfigurationPackageData Deserialize(string data);
}