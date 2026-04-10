using FluentAssertions;
using Xunit;


namespace Web.AcceptanceTests.Tests.Api;

public sealed partial class ApiRootControllerTests
{
    [Fact]
    public async Task ShouldReturnApiModulesForGet()
    {
        // Given

        // When
        string actualContent = await GetAsync();

        // Then
        actualContent.Should().Contain("\"name\":\"AppSecurity\"");
        actualContent.Should().Contain("\"name\":\"ContentManagement\"");
        actualContent.Should().Contain("\"name\":\"Core\"");
        actualContent.Should().Contain("\"name\":\"DocumentManagement\"");
        actualContent.Should().Contain("\"name\":\"Logging\"");
        actualContent.Should().Contain("\"name\":\"Mail\"");
        actualContent.Should().Contain("\"name\":\"Scheduling\"");
        actualContent.Should().Contain("\"name\":\"Security\"");
        actualContent.Should().Contain("\"swaggerDef\":\"/swagger/Core/swagger.json\"");
        actualContent.Should().Contain("\"swaggerDef\":\"/swagger/AppSecurity/swagger.json\"");
        actualContent.Should().Contain("\"swaggerDef\":\"/swagger/ContentManagement/swagger.json\"");
        actualContent.Should().Contain("\"swaggerDef\":\"/swagger/DocumentManagement/swagger.json\"");
        actualContent.Should().Contain("\"swaggerDef\":\"/swagger/Logging/swagger.json\"");
        actualContent.Should().Contain("\"swaggerDef\":\"/swagger/Mail/swagger.json\"");
        actualContent.Should().Contain("\"swaggerDef\":\"/swagger/Scheduling/swagger.json\"");
        actualContent.Should().Contain("\"swaggerDef\":\"/swagger/Security/swagger.json\"");
        actualContent.Should().Contain("\"swaggerDef\":\"/swagger/Workflow/swagger.json\"");
        actualContent.Should().Contain("\"name\":\"Workflow\"");
    }
}



