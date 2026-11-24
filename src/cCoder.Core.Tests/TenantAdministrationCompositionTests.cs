// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System;
using System.Linq;
using System.Threading.Tasks;

using cCoder.ContentManagement.Exposures;
using cCoder.Core.Exposures;
using cCoder.Core.Models;
using cCoder.Core.Services.Aggregations.Security;
using cCoder.Data.Models.CMS;
using cCoder.Security.Exposures;
using cCoder.Security.Models.Entities;
using cCoder.Security.Models.Exceptions;
using cCoder.Security.Models;
using cCoder.Security.Services.Aggregations.Interfaces;

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

using Moq;

using Xunit;

namespace cCoder.Core.Tests;

public sealed partial class TenantAdministrationCompositionTests
{
    [Fact]
    public void AddCoreWeb_GivenSecurityOnly_DoesNotRegisterCoreOverride()
    {
        // Given
        CoreConfiguration configuration = CoreConfigurationFactory.Create();
        configuration.Security = new SecurityConfiguration();

        IServiceCollection services = new ServiceCollection();

        // When
        services.AddCoreWeb(configuration: configuration);

        // Then
        services
            .Should()
            .NotContain(predicate: descriptor =>
                descriptor.ImplementationType ==
                    typeof(CoreTenantAdministrationManager));

        services
            .Should()
            .ContainSingle(predicate: descriptor =>
                descriptor.ServiceType == typeof(ITenantAdministrationManager));
    }

    [Fact]
    public void AddCoreWeb_GivenSecurityWithoutContentManagement_DoesNotRequireAppManager()
    {
        // Given
        CoreConfiguration configuration = CoreConfigurationFactory.Create();
        configuration.Security = new SecurityConfiguration();

        IServiceCollection services = new ServiceCollection();

        // When
        services.AddCoreWeb(configuration: configuration);

        // Then
        services
            .Should()
            .NotContain(predicate: descriptor =>
                descriptor.ServiceType == typeof(IAppManager));
    }

    [Fact]
    public void AddCoreWeb_GivenSecurityAndContentManagement_ReplacesAdministrationManager()
    {
        // Given
        CoreConfiguration configuration = CoreConfigurationFactory.Create();
        configuration.Security = new SecurityConfiguration();
        configuration.ContentManagement = new();

        IServiceCollection services = new ServiceCollection();

        // When
        services.AddCoreWeb(configuration: configuration);

        // Then
        ServiceDescriptor descriptor = services
            .Single(predicate: descriptor =>
                descriptor.ServiceType == typeof(ITenantAdministrationManager));

        descriptor.ImplementationType
            .Should()
            .Be(expected: typeof(CoreTenantAdministrationManager));

        descriptor.Lifetime
            .Should()
            .Be(expected: ServiceLifetime.Transient);
    }

    [Fact]
    public void AddCoreHostedServices_GivenSecurityOnly_DoesNotRegisterCoreOverride()
    {
        // Given
        CoreConfiguration configuration = CoreConfigurationFactory.Create();
        configuration.Security = new SecurityConfiguration();

        IServiceCollection services = new ServiceCollection();

        // When
        services.AddCoreHostedServices(configuration: configuration);

        // Then
        services
            .Should()
            .NotContain(predicate: descriptor =>
                descriptor.ImplementationType ==
                    typeof(CoreTenantAdministrationManager));
    }

    [Fact]
    public void AddCoreHostedServices_GivenSecurityAndContentManagement_ReplacesAdministrationManager()
    {
        // Given
        CoreConfiguration configuration = CoreConfigurationFactory.Create();
        configuration.Security = new SecurityConfiguration();
        configuration.ContentManagement = new();

        IServiceCollection services = new ServiceCollection();

        // When
        services.AddCoreHostedServices(configuration: configuration);

        // Then
        services
            .Should()
            .ContainSingle(predicate: descriptor =>
                descriptor.ServiceType == typeof(ITenantAdministrationManager)
                && descriptor.ImplementationType ==
                    typeof(CoreTenantAdministrationManager));
    }

    [Fact]
    public async Task DeleteTenant_GivenAttachedApplication_RejectsBeforeSecurityDelete()
    {
        // Given
        Tenant tenant = new() { Id = "tenant-1" };
        Mock<ITenantAggregationService> aggregationService = new();
        Mock<IAppManager> appManager = new();

        appManager
            .Setup(expression: manager => manager.GetAll(ignoreFilters: true))
            .Returns(value: new[]
            {
                new App { TenantId = tenant.Id }
            }.AsQueryable());

        TenantAdministrationAggregationService manager = new(
            tenantAggregationService: aggregationService.Object,
            appManager: appManager.Object);

        // When
        Func<Task> action = async () =>
            await manager.DeleteTenantAsync(deletedTenant: tenant);

        // Then
        await action
            .Should()
            .ThrowAsync<SecurityAggregationValidationException>();

        aggregationService.Verify(
            expression: service =>
                service.DeleteTenantAsync(item: tenant),
            times: Times.Never);
    }

    [Fact]
    public async Task DeleteTenant_GivenNoAttachedApplication_DelegatesToSecurity()
    {
        // Given
        Tenant tenant = new() { Id = "tenant-1" };
        Mock<ITenantAggregationService> aggregationService = new();
        Mock<IAppManager> appManager = new();

        appManager
            .Setup(expression: manager => manager.GetAll(ignoreFilters: true))
            .Returns(value: new[]
            {
                new App { TenantId = "other-tenant" }
            }.AsQueryable());

        TenantAdministrationAggregationService manager = new(
            tenantAggregationService: aggregationService.Object,
            appManager: appManager.Object);

        // When
        await manager.DeleteTenantAsync(deletedTenant: tenant);

        // Then
        aggregationService.Verify(
            expression: service =>
                service.DeleteTenantAsync(item: tenant),
            times: Times.Once);
    }

    [Fact]
    public void CoreOverride_ResolvesWithValidationEnabled()
    {
        // Given
        IServiceCollection services = new ServiceCollection();

        services.AddTransient<ITenantAggregationService>(
            implementationFactory: _ => Mock.Of<ITenantAggregationService>());

        services.AddTransient<IAppManager>(
            implementationFactory: _ => Mock.Of<IAppManager>());

        services.AddTransient<
            ITenantAdministrationAggregationService,
            TenantAdministrationAggregationService>();

        services.AddTransient<ITenantAdministrationManager, CoreTenantAdministrationManager>();

        // When
        using ServiceProvider provider = services.BuildServiceProvider(
            options: new ServiceProviderOptions
            {
                ValidateOnBuild = true,
                ValidateScopes = true
            });

        ITenantAdministrationManager manager = provider
            .GetRequiredService<ITenantAdministrationManager>();

        // Then
        manager
            .Should()
            .BeOfType<CoreTenantAdministrationManager>();
    }
}