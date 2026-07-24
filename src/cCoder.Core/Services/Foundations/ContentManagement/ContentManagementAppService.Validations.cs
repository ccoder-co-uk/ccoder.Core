// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Dependencies;
using cCoder.Data.Models.CMS;

namespace cCoder.Core.Services.Foundations.ContentManagement;

internal sealed partial class ContentManagementAppService
{
    private static void ValidateAppOnGet(int appId, bool ignoreFilters) =>
        ValidationRulesEngine.Validate(inputs: [appId, ignoreFilters]);

    private static void ValidateAppByDomainOnGet(
        string domain,
        bool ignoreFilters) =>
        ValidationRulesEngine.Validate(inputs: [domain, ignoreFilters]);

    private static void ValidateAppsOnGet(bool ignoreFilters) =>
        ValidationRulesEngine.Validate(inputs: [ignoreFilters]);

    private static void ValidateAppOnAdd(App newApp) =>
        ValidationRulesEngine.Validate(inputs: [newApp]);

    private static void ValidateAppOnUpdate(App updatedApp) =>
        ValidationRulesEngine.Validate(inputs: [updatedApp]);

    private static void ValidateAppOnDelete(int appId) =>
        ValidationRulesEngine.Validate(inputs: [appId]);
}