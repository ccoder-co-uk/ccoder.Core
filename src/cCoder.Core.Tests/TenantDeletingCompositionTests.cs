// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System;
using System.Linq;
using System.Threading.Tasks;
using cCoder.Core.Brokers.ContentManagement;
using cCoder.Core.Models;
using cCoder.Core.Events;
using cCoder.Eventing;
using cCoder.Eventing.Brokers;
using cCoder.Eventing.Models.Exceptions;
using cCoder.Data.Models.CMS;
using cCoder.Security.Models;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace cCoder.Core.Tests;

public sealed partial class TenantDeletingCompositionTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void TenantDeletingHandlerRegisteredOnlyWhenBothDomainsEnabled(
        bool hostedServices,
        bool contentManagement)
    {
        // Given
        CoreConfiguration configuration = CoreConfigurationFactory.Create();
        configuration.Security = new SecurityConfiguration();

        if (contentManagement)
        {
            configuration.ContentManagement = new();
        }

        IServiceCollection services = new ServiceCollection();

        // When
        if (hostedServices)
        {
            services.AddCoreHostedServices(configuration: configuration);
        }
        else
        {
            services.AddCoreWeb(configuration: configuration);
        }

        // Then
        Assert.Equal(
            expected: contentManagement ? 1 : 0,
            actual: services.Count(predicate: descriptor =>
                descriptor.ServiceType == typeof(ILocalEventHandler<string>)
                && descriptor.IsKeyedService
                && descriptor.ServiceKey is "tenant_delete"
                && descriptor.KeyedImplementationType == typeof(TenantAppDeletingHandler)));
    }

    [Theory]
    [InlineData(false, "tenant-1", true)]
    [InlineData(false, "tenant-2", false)]
    [InlineData(true, "tenant-1", true)]
    [InlineData(true, "tenant-2", false)]
    public async Task AppHandlerRejectsOnlyMatchingTenant(
        bool hostedServices, string appTenantId, bool shouldReject)
    {
        // Given
        Mock<IContentManagementAppBroker> appBroker = new(MockBehavior.Strict);

        appBroker.Setup(expression: broker => broker.GetAllApps(ignoreFilters: true))
            .Returns(value: new[] { new App { TenantId = appTenantId } }.AsQueryable());

        CoreConfiguration configuration = CoreConfigurationFactory.Create();
        configuration.Security = new SecurityConfiguration();
        configuration.ContentManagement = new();
        IServiceCollection services = new ServiceCollection();

        if (hostedServices)
        {
            services.AddCoreHostedServices(configuration: configuration);
        }
        else
        {
            services.AddCoreWeb(configuration: configuration);
        }

        services.AddSingleton<IContentManagementAppBroker>(implementationInstance: appBroker.Object);
        using ServiceProvider provider = services.BuildServiceProvider();
        using IServiceScope scope = provider.CreateScope();
        ILocalEventBroker broker = scope.ServiceProvider.GetRequiredService<ILocalEventBroker>();

        // When
        Func<Task> handle = async () => await broker.DispatchAsync(name: "tenant_delete", data: "tenant-1");

        // Then
        if (shouldReject)
        {
            LocalEventRejectedException rejection =
                await Assert.ThrowsAsync<LocalEventRejectedException>(testCode: handle);

            Assert.Equal(expected: "linked_apps", actual: rejection.Code);
        }
        else
        {
            await handle();
        }
    }
}