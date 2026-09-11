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

public sealed class CorePackageProcessingServiceTests
{
    [Fact]
    public async Task ImportPackageAsync_WhenCoreAppItemsExist_ImportsEachInSourceOrder()
    {
        List<string> importedData = [];
        Mock<ICorePackageService> corePackageServiceMock = new();

        corePackageServiceMock
            .Setup(service => service.ImportAppConfigurationAsync(
                It.IsAny<int>(),
                It.IsAny<string>()))
            .Callback<int, string>((_, data) => importedData.Add(item: data))
            .Returns(ValueTask.CompletedTask);

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

        await service.ImportPackageAsync(appId: 42, package: package);

        importedData.Should().Equal("first", "second");
    }

    [Fact]
    public async Task ImportPackageAsync_WhenNoCoreAppItemsExist_DoesNotCallFoundation()
    {
        Mock<ICorePackageService> corePackageServiceMock = new();
        CorePackageProcessingService service = new(
            corePackageService: corePackageServiceMock.Object);

        await service.ImportPackageAsync(
            appId: 42,
            package: new Package
            {
                Items = [new PackageItem { Type = "Other/Item", Data = "ignored" }],
            });

        corePackageServiceMock.Verify(
            expression: service => service.ImportAppConfigurationAsync(
                It.IsAny<int>(),
                It.IsAny<string>()),
            times: Times.Never);
    }

    [Fact]
    public async Task ExportMethods_WhenCalled_DelegateToMatchingFoundationOperation()
    {
        Package appPackage = new() { Name = "AppConfiguration" };
        Package pageRolesPackage = new() { Name = "PageRoles" };
        Package folderRolesPackage = new() { Name = "FolderRoles" };
        Mock<ICorePackageService> corePackageServiceMock = new();

        corePackageServiceMock.Setup(service => service.ExportAppConfigurationAsync(42, "source"))
            .ReturnsAsync(appPackage);
        corePackageServiceMock.Setup(service => service.ExportPageRolesAsync(42, "source"))
            .ReturnsAsync(pageRolesPackage);
        corePackageServiceMock.Setup(service => service.ExportFolderRolesAsync(42, "source"))
            .ReturnsAsync(folderRolesPackage);

        CorePackageProcessingService service = new(
            corePackageService: corePackageServiceMock.Object);

        Package actualApp = await service.ExportAppConfigurationAsync(42, "source");
        Package actualPageRoles = await service.ExportPageRolesAsync(42, "source");
        Package actualFolderRoles = await service.ExportFolderRolesAsync(42, "source");

        actualApp.Should().BeSameAs(appPackage);
        actualPageRoles.Should().BeSameAs(pageRolesPackage);
        actualFolderRoles.Should().BeSameAs(folderRolesPackage);
    }
}