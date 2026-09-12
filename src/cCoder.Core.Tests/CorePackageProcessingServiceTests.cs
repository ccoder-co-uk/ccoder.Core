// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Services.Foundations.Packages;
using cCoder.Core.Services.Processings.Packages;
using cCoder.Data.Models.Packaging;
using FluentAssertions;
using Moq;
using Xunit;

namespace cCoder.Core.Tests;

public sealed partial class CorePackageProcessingServiceTests
{
    [Fact]
    public async Task ImportPackageAsync_WhenCoreAppItemsExist_ImportsEachInSourceOrder()
    {
        // Given
        List<string> importedData = [];
        Mock<ICorePackageService> corePackageServiceMock = new();

        corePackageServiceMock
            .Setup(expression: service => service.ImportAppConfigurationAsync(
                appId: It.IsAny<int>(),
                data: It.IsAny<string>()))
            .Callback<int, string>(action: (_, data) => importedData.Add(item: data))
            .Returns(value: ValueTask.CompletedTask);

        CorePackageProcessingService service = new(
            corePackageService: corePackageServiceMock.Object);

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
        await service.ImportPackageAsync(
            appId: 42,
            package: package);

        // Then
        importedData.Count
            .Should()
            .Be(expected: 2);

        importedData[0]
            .Should()
            .Be(expected: "first");

        importedData[1]
            .Should()
            .Be(expected: "second");
    }

    [Fact]
    public async Task ImportPackageAsync_WhenNoCoreAppItemsExist_DoesNotCallFoundation()
    {
        // Given
        Mock<ICorePackageService> corePackageServiceMock = new();

        CorePackageProcessingService service = new(
            corePackageService: corePackageServiceMock.Object);

        Package package = new()
        {
            Items =
            [
                new PackageItem
                {
                    Type = "Other/Item",
                    Data = "ignored",
                },
            ],
        };

        // When
        await service.ImportPackageAsync(
            appId: 42,
            package: package);

        // Then
        corePackageServiceMock.Verify(
            expression: service => service.ImportAppConfigurationAsync(
                appId: It.IsAny<int>(),
                data: It.IsAny<string>()),
            times: Times.Never);
    }

    [Fact]
    public async Task ExportMethods_WhenCalled_DelegateToMatchingFoundationOperation()
    {
        // Given
        Package appPackage = new() { Name = "AppConfiguration" };
        Package pageRolesPackage = new() { Name = "PageRoles" };
        Package folderRolesPackage = new() { Name = "FolderRoles" };
        Mock<ICorePackageService> corePackageServiceMock = new();

        corePackageServiceMock
            .Setup(expression: service => service.ExportAppConfigurationAsync(
                appId: 42,
                sourceApi: "source"))
            .ReturnsAsync(value: appPackage);

        corePackageServiceMock
            .Setup(expression: service => service.ExportPageRolesAsync(
                appId: 42,
                sourceApi: "source"))
            .ReturnsAsync(value: pageRolesPackage);

        corePackageServiceMock
            .Setup(expression: service => service.ExportFolderRolesAsync(
                appId: 42,
                sourceApi: "source"))
            .ReturnsAsync(value: folderRolesPackage);

        CorePackageProcessingService service = new(
            corePackageService: corePackageServiceMock.Object);

        // When
        Package actualApp = await service.ExportAppConfigurationAsync(
            appId: 42,
            sourceApi: "source");

        Package actualPageRoles = await service.ExportPageRolesAsync(
            appId: 42,
            sourceApi: "source");

        Package actualFolderRoles = await service.ExportFolderRolesAsync(
            appId: 42,
            sourceApi: "source");

        // Then
        actualApp
            .Should()
            .BeSameAs(expected: appPackage);

        actualPageRoles
            .Should()
            .BeSameAs(expected: pageRolesPackage);

        actualFolderRoles
            .Should()
            .BeSameAs(expected: folderRolesPackage);
    }
}