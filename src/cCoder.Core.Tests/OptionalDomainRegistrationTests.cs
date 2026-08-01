// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Models;
using cCoder.AppSecurity.Models;
using cCoder.ContentManagement.Models;
using cCoder.Data.Models;
using cCoder.DocumentManagement.Models;
using cCoder.Logging.Models;
using cCoder.Mail.Models;
using cCoder.Packaging.Models;
using cCoder.Security.Models;
using cCoder.Workflow.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.OData;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Swashbuckle.AspNetCore.SwaggerGen;
using Xunit;

namespace cCoder.Core.Tests;

public sealed partial class OptionalDomainRegistrationTests
{
    [Fact]
    public void AddCoreWeb_ShouldNotAdvertiseOmittedDomains()
    {
        // Given
        IConfiguration applicationConfiguration =
            new ConfigurationBuilder()
                .Build();

        CoreConfiguration configuration =
            new(applicationConfiguration);

        IServiceCollection services = new ServiceCollection();

        services.AddSingleton<IWebHostEnvironment>(
            implementationInstance: Mock.Of<IWebHostEnvironment>());

        // When
        services.AddCoreWeb(configuration: configuration);

        // Then
        services
            .Should()
            .NotContain(predicate: descriptor =>
                descriptor.ServiceType == typeof(ApiInfo));

        Type[] domainConfigurationTypes =
        [
            typeof(AppSecurityConfiguration),
            typeof(ContentManagementConfiguration),
            typeof(DocumentManagementConfiguration),
            typeof(LoggingConfiguration),
            typeof(MailConfiguration),
            typeof(PackagingConfiguration),
            typeof(SecurityConfiguration),
            typeof(WorkflowConfiguration)
        ];

        services
            .Should()
            .NotContain(predicate: descriptor =>
                domainConfigurationTypes.Contains(
                    value: descriptor.ServiceType));

        using ServiceProvider serviceProvider =
            services.BuildServiceProvider();

        SwaggerGeneratorOptions swaggerOptions = serviceProvider
            .GetRequiredService<IOptions<SwaggerGeneratorOptions>>()
            .Value;

        swaggerOptions.SwaggerDocs.Keys
            .Should()
            .Equal(expected: ["Core"]);

        ODataOptions oDataOptions = serviceProvider
            .GetRequiredService<IOptions<ODataOptions>>()
            .Value;

        oDataOptions.RouteComponents.Keys
            .Should()
            .NotContain(unexpected: "Api/Packaging");
    }

    [Fact]
    public void AddCoreWeb_ShouldOnlyAdvertiseEnabledDomains()
    {
        // Given
        IConfiguration applicationConfiguration = new ConfigurationBuilder()
            .AddInMemoryCollection(initialData: new Dictionary<string, string>
            {
                ["Packaging:AssetsRoot"] = "https://assets.example/"
            })
            .Build();

        CoreConfiguration configuration =
            new(applicationConfiguration);

        IServiceCollection services = new ServiceCollection();

        services.AddSingleton<IWebHostEnvironment>(
            implementationInstance: Mock.Of<IWebHostEnvironment>());

        // When
        services.AddCoreWeb(configuration: configuration);

        // Then
        string[] apiContextNames =
        [.. services.Where(predicate: descriptor =>
                descriptor.ServiceType == typeof(ApiInfo))
            .Select(selector: descriptor => descriptor.ImplementationInstance)
            .OfType<ApiInfo>()
            .Select(selector: info => info.Name)];

        apiContextNames
            .Should()
            .Equal(expected: ["Packaging"]);

        services
            .Should()
            .Contain(predicate: descriptor =>
                descriptor.ServiceType == typeof(PackagingConfiguration));

        using ServiceProvider serviceProvider =
            services.BuildServiceProvider();

        SwaggerGeneratorOptions swaggerOptions = serviceProvider
            .GetRequiredService<IOptions<SwaggerGeneratorOptions>>()
            .Value;

        swaggerOptions.SwaggerDocs.Keys
            .Should()
            .Equal(expected: ["Core", "Packaging"]);

        ODataOptions oDataOptions = serviceProvider
            .GetRequiredService<IOptions<ODataOptions>>()
            .Value;

        oDataOptions.RouteComponents.Keys
            .Should()
            .Contain(expected: "Api/Packaging");
    }
}