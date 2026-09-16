// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Dependencies;
using cCoder.Data.Models.CMS;

namespace cCoder.Core.Services.Foundations.ContentManagement;

internal sealed partial class ContentManagementAppService
{
    private static void Validate(params object[] inputs) =>
        ValidationRulesEngine.Validate(inputs: inputs);

    private static void ValidateAppOnGet(int appId, bool ignoreFilters) =>
        Validate(inputs: [appId, ignoreFilters]);

    private static void ValidateAppByDomainOnGet(
        string domain,
        bool ignoreFilters) =>
        Validate(inputs: [domain, ignoreFilters]);

    private static void ValidateAllAppsOnGet(bool ignoreFilters) =>
        Validate(inputs: [ignoreFilters]);

    private static void ValidateAllAppsWithTemplatesOnGet(bool ignoreFilters) =>
        Validate(inputs: [ignoreFilters]);

    private static void ValidateAppOnAdd(App newApp) =>
        Validate(inputs: [newApp]);

    private static void ValidateAppOnUpdate(App updatedApp) =>
        Validate(inputs: [updatedApp]);

    private static void ValidateAppOnDelete(int appId) =>
        Validate(inputs: [appId]);
}