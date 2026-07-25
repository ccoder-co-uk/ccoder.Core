// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Services.Foundations.Setup;
using cCoder.Core.Services.Orchestrations;
using Moq;

namespace cCoder.Core.Tests;

public sealed partial class FirstTimeSetupStateOrchestrationServiceTests
{
    private readonly Mock<ICoreSetupStateService> coreSetupStateServiceMock =
        new();

    private readonly Mock<ISecuritySetupStateService>
        securitySetupStateServiceMock =
            new();

    private readonly IFirstTimeSetupStateOrchestrationService
        firstTimeSetupStateOrchestrationService;

    public FirstTimeSetupStateOrchestrationServiceTests()
    {
        firstTimeSetupStateOrchestrationService =
            new FirstTimeSetupStateOrchestrationService(
                coreSetupStateService:
                    coreSetupStateServiceMock.Object,
                securitySetupStateService:
                    securitySetupStateServiceMock.Object);
    }
}