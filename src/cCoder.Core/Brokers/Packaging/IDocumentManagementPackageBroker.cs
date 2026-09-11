// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.DocumentManagement.Models;

namespace cCoder.Core.Brokers.Packaging;

internal interface IDocumentManagementPackageBroker
{
    ValueTask ImportPackageAsync(int appId, DocumentManagementPackage documentManagementPackage);
    DocumentManagementPackage ExportPackage(int appId, string packageName);
}