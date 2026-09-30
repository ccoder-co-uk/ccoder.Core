// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;
namespace cCoder.Core.Services.Foundations.Setup;

internal interface ISecuritySetupStateService
{
    ValueTask<bool> IsSecurityInitializedAsync(
        CancellationToken cancellationToken = default);
}