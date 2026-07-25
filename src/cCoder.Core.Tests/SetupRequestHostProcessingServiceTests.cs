// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Services.Processings.Setup;

namespace cCoder.Core.Tests;

public sealed partial class SetupRequestHostProcessingServiceTests
{
    private readonly ISetupRequestHostProcessingService setupRequestHostProcessingService =
        new SetupRequestHostProcessingService();
}