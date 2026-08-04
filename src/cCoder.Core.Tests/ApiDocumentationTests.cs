// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Swashbuckle.AspNetCore.SwaggerGen;
using Xunit;

namespace cCoder.Core.Tests;

public sealed partial class ApiDocumentationTests
{
    [Fact]
    public void ShouldRegisterNamedSwaggerDocumentsWithoutLegacyV1Alias()
    {
        // Given
        ServiceCollection services = new();

        services.AddSingleton<IWebHostEnvironment>(
            implementationInstance: Mock.Of<IWebHostEnvironment>());

        CoreApiRouteDefinition[] routes =
        [
            new() { Name = "AI", RoutePath = "Api/AI", RouteModel = null },
            new() { Name = "Core", RoutePath = "Api/Core", RouteModel = null },
            new() { Name = "ContentManagement", RoutePath = "Api/ContentManagement", RouteModel = null },
        ];

        // When
        services.AddCoreApiDocumentation(routes: routes);

        using ServiceProvider serviceProvider = services.BuildServiceProvider();

        SwaggerGeneratorOptions options = serviceProvider
            .GetRequiredService<IOptions<SwaggerGeneratorOptions>>()
            .Value;

        // Then
        options.SwaggerDocs.Keys.Should()
            .BeEquivalentTo(expectation: ["AI", "Core", "ContentManagement"]);

        options.SwaggerDocs.Keys.Should()
            .NotContain(unexpected: "v1");
    }

    [Fact]
    public void ShouldPartitionOperationsIntoNamedSwaggerDocuments()
    {
        // Given
        ServiceCollection services = new();

        services.AddSingleton<IWebHostEnvironment>(
            implementationInstance: Mock.Of<IWebHostEnvironment>());

        CoreApiRouteDefinition[] routes =
        [
            new() { Name = "AI", RoutePath = "Api/AI", RouteModel = null },
            new() { Name = "Core", RoutePath = "Api/Core", RouteModel = null },
            new() { Name = "ContentManagement", RoutePath = "Api/ContentManagement", RouteModel = null },
        ];

        services.AddCoreApiDocumentation(routes: routes);

        using ServiceProvider serviceProvider = services.BuildServiceProvider();

        SwaggerGeneratorOptions options = serviceProvider
            .GetRequiredService<IOptions<SwaggerGeneratorOptions>>()
            .Value;

        // When
        bool coreIncludesCoreOperation = options.DocInclusionPredicate.Invoke(
            arg1: "Core",
            arg2: new() { RelativePath = "Api/Core/App" });

        bool aiIncludesModelOperation = options.DocInclusionPredicate.Invoke(
            arg1: "AI",
            arg2: new() { RelativePath = "Api/AI/Model/Providers/Ollama/Available" });

        bool coreIncludesAiOperation = options.DocInclusionPredicate.Invoke(
            arg1: "Core",
            arg2: new() { RelativePath = "Api/AI/Model/Providers/Ollama/Available" });

        bool coreIncludesContentManagementOperation = options.DocInclusionPredicate.Invoke(
            arg1: "Core",
            arg2: new() { RelativePath = "Api/ContentManagement/Page" });

        bool contentManagementIncludesOperation = options.DocInclusionPredicate.Invoke(
            arg1: "ContentManagement",
            arg2: new() { RelativePath = "Api/ContentManagement/Page" });

        // Then
        coreIncludesCoreOperation
            .Should()
            .BeTrue();

        aiIncludesModelOperation
            .Should()
            .BeTrue();

        coreIncludesAiOperation
            .Should()
            .BeFalse();

        coreIncludesContentManagementOperation
            .Should()
            .BeFalse();

        contentManagementIncludesOperation
            .Should()
            .BeTrue();
    }
}