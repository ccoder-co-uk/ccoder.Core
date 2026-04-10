using System.Net;
using FluentAssertions;
using Xunit;


namespace Web.AcceptanceTests.Tests.Api;

public sealed partial class SwaggerMiddlewareTests
{
    [Theory]
    [InlineData("/swagger/Core/swagger.json")]
    [InlineData("/swagger/AppSecurity/swagger.json")]
    [InlineData("/swagger/ContentManagement/swagger.json")]
    [InlineData("/swagger/DocumentManagement/swagger.json")]
    [InlineData("/swagger/Logging/swagger.json")]
    [InlineData("/swagger/Mail/swagger.json")]
    [InlineData("/swagger/Scheduling/swagger.json")]
    [InlineData("/swagger/Security/swagger.json")]
    [InlineData("/swagger/Workflow/swagger.json")]
    [InlineData("/swagger/v1/swagger.json")]
    public async Task Invoke_ReturnsSwaggerDefinition(string baseUrl)
    {
        // Given
        int actualStatusCode;

        // When
        actualStatusCode = await InvokeAsync(baseUrl);

        // Then
        actualStatusCode.Should().Be((int)HttpStatusCode.OK);
    }
}



