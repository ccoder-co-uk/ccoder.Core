// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Models;
using cCoder.Eventing.Models;
using cCoder.Security.Models.Configurations;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace cCoder.Core.Tests;

public sealed partial class EventAuthInfoRegistrationTests
{
    [Fact]
    public void AddCoreWeb_ShouldProvideEventAuthInfoFromSecurityAuthInfo()
    {
        // Given
        IServiceCollection services = CreateServices();

        services.AddSingleton<IWebHostEnvironment>(
            implementationInstance: Mock.Of<IWebHostEnvironment>());

        // When
        services.AddCoreWeb(
            configuration: CoreConfigurationFactory.Create());

        AddSecurityAuthInfo(services: services);

        // Then
        AssertEventAuthInfoUsesSecurityAuthInfo(services: services);
    }

    [Fact]
    public void AddCoreHostedServices_ShouldProvideEventAuthInfoFromSecurityAuthInfo()
    {
        // Given
        IServiceCollection services = CreateServices();

        // When
        services.AddCoreHostedServices(
            configuration: CoreConfigurationFactory.Create());

        AddSecurityAuthInfo(services: services);

        // Then
        AssertEventAuthInfoUsesSecurityAuthInfo(services: services);
    }

    private static IServiceCollection CreateServices()
    {
        IServiceCollection services = new ServiceCollection();

        return services;
    }

    private static void AddSecurityAuthInfo(
        IServiceCollection services)
    {
        Mock<ISSOAuthInfo> authInfoMock = new();

        authInfoMock.SetupGet(
            expression: authInfo => authInfo.SSOUserId)
            .Returns(value: "security-user-id");

        services.AddSingleton(
            implementationInstance: authInfoMock.Object);
    }

    private static void AssertEventAuthInfoUsesSecurityAuthInfo(
        IServiceCollection services)
    {
        services.Should()
            .Contain(predicate: descriptor =>
                descriptor.ServiceType == typeof(IEventAuthInfo));

        using ServiceProvider serviceProvider =
            services.BuildServiceProvider();

        serviceProvider.GetRequiredService<IEventAuthInfo>()
            .SSOUserId
            .Should()
            .Be(expected: "security-user-id");
    }
}