// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Exposures;
using cCoder.Core.Brokers.Http;
using cCoder.Core.Services.Aggregations.Packages;
using cCoder.Data.Models.Packaging;
using cCoder.Packaging.Brokers.PackageTransfers;

namespace cCoder.Core.Exposures.PackageManagers;

internal sealed class PackageTransferManager(
    IPackageManagerAggregationService packageManagerAggregationService,
    IHttpRequestBroker httpRequestBroker)
    : IPackageTransferBroker, ICompositionExposure
{
    public string GetRequestDomain() =>
        httpRequestBroker.GetCurrentRequest()?.Host.Host
        ?? throw new InvalidOperationException(
            message: "Package export requires an active HTTP request.");

    public ValueTask<Package[]> ExportPackagesAsync(
        int appId,
        string[] packageNames,
        string sourceApi) =>
        packageManagerAggregationService.ExportPackagesAsync(
            appId: appId,
            packageNames: packageNames,
            sourceApi: sourceApi);
}