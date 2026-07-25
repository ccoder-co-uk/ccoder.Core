// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Dependencies;
using cCoder.Data.Models.CMS;

namespace cCoder.Core.Services.Foundations.Eventing;

internal sealed partial class AppGraphEventService
{
    private static void ValidateAppOnRaise(App app) =>
        ValidationRulesEngine.Validate(inputs: [app]);
}