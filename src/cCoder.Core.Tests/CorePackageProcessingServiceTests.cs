// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Brokers.Json;
using cCoder.Core.Models.Packaging;
using cCoder.Core.Services.Foundations.ContentManagement;
using cCoder.Core.Services.Processings.Packages;
using cCoder.Data.Models.CMS;
using cCoder.Data.Models.Packaging;
using FluentAssertions;
using Moq;
using Xunit;

namespace cCoder.Core.Tests;

public sealed partial class ContentManagementAppPackageProcessingServiceTests
{
    [Fact]
    public async Task ImportPackageAsync_WhenCoreAppItemsExist_UpdatesEachInSourceOrder()
    {
        // Given
        List<string> updatedNames = [];
        Mock<IContentManagementAppService> appServiceMock = new();
        Mock<ICorePackageJsonBroker> jsonBrokerMock = new();
        App app = new() { Id = 42, Name = "Original" };

        appServiceMock
            .Setup(expression: service => service.GetApp(
                appId: 42,
                ignoreFilters: true))
            .Returns(value: app);

        appServiceMock
            .Setup(expression: service => service.UpdateAppAsync(updatedApp: app))
            .Callback<App>(action: updatedApp => updatedNames.Add(item: updatedApp.Name))
            .ReturnsAsync(value: app);

        jsonBrokerMock
            .Setup(expression: broker => broker.Deserialize(data: "first"))
            .Returns(value: new AppConfigurationPackageData { Name = "First" });

        jsonBrokerMock
            .Setup(expression: broker => broker.Deserialize(data: "second"))
            .Returns(value: new AppConfigurationPackageData { Name = "Second" });

        ContentManagementAppPackageProcessingService service = new(
            contentManagementAppService: appServiceMock.Object,
            corePackageJsonBroker: jsonBrokerMock.Object);

        Package package = new()
        {
            Items =
            [
                new PackageItem { Type = "Other/Item", Data = "ignored" },
                new PackageItem { Type = "core/app", Data = "first" },
                new PackageItem { Type = "CORE/APP", Data = "second" },
            ],
        };

        // When
        await service.ImportPackageAsync(appId: 42, package: package);

        // Then
        updatedNames.Count
            .Should()
            .Be(expected: 2);

        updatedNames[0]
            .Should()
            .Be(expected: "First");

        updatedNames[1]
            .Should()
            .Be(expected: "Second");
    }

    [Fact]
    public async Task ImportPackageAsync_WhenNoCoreAppItemsExist_DoesNotLoadApp()
    {
        // Given
        Mock<IContentManagementAppService> appServiceMock = new();
        Mock<ICorePackageJsonBroker> jsonBrokerMock = new();

        ContentManagementAppPackageProcessingService service = new(
            contentManagementAppService: appServiceMock.Object,
            corePackageJsonBroker: jsonBrokerMock.Object);

        Package package = new()
        {
            Items = [new PackageItem { Type = "Other/Item", Data = "ignored" }],
        };

        // When
        await service.ImportPackageAsync(appId: 42, package: package);

        // Then
        appServiceMock.Verify(
            expression: service => service.GetApp(
                appId: It.IsAny<int>(),
                ignoreFilters: It.IsAny<bool>()),
            times: Times.Never);
    }

    [Fact]
    public async Task ExportAppConfigurationAsync_WhenCalled_ReturnsCurrentAppPackage()
    {
        // Given
        Mock<IContentManagementAppService> appServiceMock = new();
        Mock<ICorePackageJsonBroker> jsonBrokerMock = new();

        App app = new()
        {
            Id = 42,
            TenantId = "7",
            Name = "App",
            Domain = "app.test",
            DefaultCultureId = "en-GB",
            DefaultTheme = "Default",
            ConfigJson = "{}",
        };

        appServiceMock
            .Setup(expression: service => service.GetApp(
                appId: 42,
                ignoreFilters: true))
            .Returns(value: app);

        jsonBrokerMock
            .Setup(expression: broker => broker.Serialize(
                appConfigurationPackageData: It.Is<AppConfigurationPackageData>(
                    match: data => data.Id == 42 && data.Name == "App")))
            .Returns(value: "serialized");

        ContentManagementAppPackageProcessingService service = new(
            contentManagementAppService: appServiceMock.Object,
            corePackageJsonBroker: jsonBrokerMock.Object);

        // When
        Package actual = await service.ExportAppConfigurationAsync(
            appId: 42,
            sourceApi: "source");

        // Then
        actual.Name
            .Should()
            .Be(expected: "AppConfiguration");

        actual.SourceApi
            .Should()
            .Be(expected: "source");

        actual.Items
            .Should()
            .ContainSingle();

        PackageItem packageItem = actual.Items.Single();

        packageItem.Type
            .Should()
            .Be(expected: "Core/App");

        packageItem.Data
            .Should()
            .Be(expected: "serialized");
    }
}