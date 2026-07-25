// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data.Models.DMS;

namespace cCoder.Core.Services.Foundations.Eventing;

internal interface IServiceBusFolderDeleteForwardingService
{
    ValueTask ForwardFolderDeleteAsync(Folder folder);
}