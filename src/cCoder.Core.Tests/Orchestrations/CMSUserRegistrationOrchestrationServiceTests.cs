using cCoder.AppSecurity.Services.Orchestrations;
using cCoder.Core.Services.Foundations.ContentManagement;
using cCoder.Data.Models.CMS;
using cCoder.Data.Models.Security;
using cCoder.Core.Services.Orchestrations;
using Microsoft.Extensions.Logging;
using Moq;


namespace cCoder.Core.Services.Tests.CMS.Orchestrations;

public partial class CMSUserRegistrationOrchestrationServiceTests
{
    private readonly Mock<IContentManagementAppService> contentManagementAppServiceMock;
    private readonly Mock<IRoleOrchestrationService> roleOrchestrationServiceMock;
    private readonly Mock<IUserOrchestrationService> userOrchestrationServiceMock;
    private readonly Mock<IUserRoleOrchestrationService> userRoleOrchestrationServiceMock;
    private readonly Mock<ITemplatedEmailOrchestrationService> templatedEmailOrchestrationServiceMock;
    private readonly Mock<ILogger<CMSUserRegistrationOrchestrationService>> loggerMock;
    private readonly CMSUserRegistrationOrchestrationService orchestrationService;

    public CMSUserRegistrationOrchestrationServiceTests()
    {
        contentManagementAppServiceMock = new Mock<IContentManagementAppService>(MockBehavior.Strict);
        roleOrchestrationServiceMock = new Mock<IRoleOrchestrationService>(MockBehavior.Strict);
        userOrchestrationServiceMock = new Mock<IUserOrchestrationService>(MockBehavior.Strict);
        userRoleOrchestrationServiceMock = new Mock<IUserRoleOrchestrationService>(
            MockBehavior.Strict
        );
        templatedEmailOrchestrationServiceMock = new Mock<ITemplatedEmailOrchestrationService>(
            MockBehavior.Strict
        );
        loggerMock = new Mock<ILogger<CMSUserRegistrationOrchestrationService>>();

        orchestrationService = new CMSUserRegistrationOrchestrationService(
            contentManagementAppServiceMock.Object,
            roleOrchestrationServiceMock.Object,
            userOrchestrationServiceMock.Object,
            userRoleOrchestrationServiceMock.Object,
            templatedEmailOrchestrationServiceMock.Object,
            loggerMock.Object
        );
    }

    private static App CreateAppWithoutEmailInfrastructure(int id) =>
        new()
        {
            Id = id,
            Name = "App",
            Roles = [],
            Cultures = [],
            MailServers = [],
            Templates = [],
            Resources = [],
        };

    private static User CreateUser() =>
        new()
        {
            Id = Guid.NewGuid().ToString("N"),
            Email = "user@example.test",
            DisplayName = "User",
            DefaultCultureId = "en-GB",
            IsActive = true,
        };
}









