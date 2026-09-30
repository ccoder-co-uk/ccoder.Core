// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;
namespace cCoder.Core.Services.Foundations.Setup;

internal interface ICoreSetupStateService
{
    ValueTask<bool> IsCoreInitializedAsync(
        CancellationToken cancellationToken = default);
}