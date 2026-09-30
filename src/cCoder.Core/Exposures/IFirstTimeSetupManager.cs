// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;
namespace cCoder.Core.Exposures;

public interface IFirstTimeSetupManager
{
    Task<bool> IsInitializedAsync(CancellationToken cancellationToken = default);
}