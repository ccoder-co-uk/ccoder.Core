// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Dependencies;
using cCoder.Data.Models.DMS;

namespace cCoder.Core.Services.Foundations.Eventing;

internal sealed partial class ServiceBusFolderDeleteForwardingService
{
    private static void ValidateFolderDeleteOnForward(Folder folder) =>
        ValidationRulesEngine.Validate(inputs: [folder]);
}