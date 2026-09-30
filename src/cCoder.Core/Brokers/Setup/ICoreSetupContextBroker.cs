// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;
namespace cCoder.Core.Brokers.Setup;

internal interface ICoreSetupContextBroker
{
    ValueTask<bool> IsInitializedAsync(CancellationToken cancellationToken);
}