// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Services.Processings.Setup;

namespace cCoder.Core.Exposures.Setup;

internal sealed class SetupRequestHostManager(
    ISetupRequestHostProcessingService setupRequestHostProcessingService)
    : ISetupRequestHostManager
{
    public string NormalizeHost(string host) =>
        setupRequestHostProcessingService.NormalizeHost(
            host: host);
}