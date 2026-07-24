// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Dependencies;
using cCoder.Data.Models.CMS;

namespace cCoder.Core.Services.Aggregations;

internal sealed partial class AppAggregationService
{
    private static void ValidateAppOnAdd(App newApp) =>
        ValidationRulesEngine.Validate(inputs: [newApp]);

    private static void ValidateAppOnUpdate(App updatedApp) =>
        ValidationRulesEngine.Validate(inputs: [updatedApp]);

    private static void ValidateAppOnDelete(int appId) =>
        ValidationRulesEngine.Validate(inputs: [appId]);
}