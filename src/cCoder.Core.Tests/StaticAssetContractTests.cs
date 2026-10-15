// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System;
using System.IO;
using Xunit;

namespace cCoder.Core.Tests;

public sealed partial class StaticAssetContractTests
{
    [Theory]
    [InlineData("treefolder.svg")]
    [InlineData("treeopenfolder.svg")]
    public void DocumentManagementFolderIconsShouldBePublished(string fileName)
    {
        // Given
        string repositoryRoot = FindRepositoryRoot();

        string assetPath = Path.Combine(paths:
        [
            repositoryRoot,
            "src",
            "cCoder.Core",
            "wwwroot",
            "icons",
            fileName
        ]);

        // When
        bool assetExists = File.Exists(path: assetPath);

        // Then
        Assert.True(
            condition: assetExists,
            userMessage: $"The published DMS folder icon is missing: {assetPath}");
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo directory = new(path: AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(path: Path.Combine(
                path1: directory.FullName,
                path2: "src",
                path3: "cCoder.Core.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException(
            message: "The cCoder.Core repository root was not found.");
    }
}