// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

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
        actualContent.Should()
            .Contain(expected: "\"name\":\"AppSecurity\"");

        actualContent.Should()
            .Contain(expected: "\"name\":\"ContentManagement\"");

        actualContent.Should()
            .NotContain(unexpected: "\"name\":\"Core\"");

        actualContent.Should()
            .Contain(expected: "\"name\":\"DocumentManagement\"");

        actualContent.Should()
            .Contain(expected: "\"name\":\"Logging\"");

        actualContent.Should()
            .Contain(expected: "\"name\":\"Mail\"");

        actualContent.Should()
            .Contain(expected: "\"name\":\"Packaging\"");

        actualContent.Should()
            .Contain(expected: "\"name\":\"Security\"");

        actualContent.Should()
            .NotContain(unexpected: "\"swaggerDef\":\"/swagger/Core/swagger.json\"");

        actualContent.Should()
            .Contain(expected: "\"swaggerDef\":\"/swagger/AppSecurity/swagger.json\"");

        actualContent.Should()
            .Contain(expected: "\"swaggerDef\":\"/swagger/ContentManagement/swagger.json\"");

        actualContent.Should()
            .Contain(expected: "\"swaggerDef\":\"/swagger/DocumentManagement/swagger.json\"");

        actualContent.Should()
            .Contain(expected: "\"swaggerDef\":\"/swagger/Logging/swagger.json\"");

        actualContent.Should()
            .Contain(expected: "\"swaggerDef\":\"/swagger/Mail/swagger.json\"");

        actualContent.Should()
            .Contain(expected: "\"swaggerDef\":\"/swagger/Packaging/swagger.json\"");

        actualContent.Should()
            .Contain(expected: "\"swaggerDef\":\"/swagger/Security/swagger.json\"");

        actualContent.Should()
            .Contain(expected: "\"swaggerDef\":\"/swagger/Workflow/swagger.json\"");

        actualContent.Should()
            .Contain(expected: "\"name\":\"Workflow\"");
    }
}