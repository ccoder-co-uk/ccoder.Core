// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;

namespace cCoder.IntegrationTests.Infrastructure;

internal static class TestConfiguration
{
    private static readonly IConfiguration configuration = BuildConfiguration();

    internal static string ReadOptionalValue(string variableName)
    {
        string environmentValue = ReadEnvironmentValue(variableName: variableName);

        return environmentValue
            ?? configuration[variableName.Replace(oldValue: "__", newValue: ":")];
    }

    internal static string ReadRequiredValue(string variableName)
    {
        string value = ReadOptionalValue(variableName: variableName);

        if (!string.IsNullOrWhiteSpace(value: value))
        {
            return value;
        }

        throw new InvalidOperationException(
            $"Required test configuration value '{variableName}' was not found in the acceptance appsettings files or environment.");
    }

    private static IConfiguration BuildConfiguration()
    {
        string repositoryRoot = FindRepositoryRoot();
        IConfigurationBuilder builder = new ConfigurationBuilder();
        _ = builder.SetBasePath(basePath: repositoryRoot);

        foreach (string relativePath in GetSettingsPaths())
        {
            string path = Path.Combine(
                path1: repositoryRoot,
                path2: relativePath);

            if (File.Exists(path: path))
            {
                _ = builder.AddJsonFile(
                    path: relativePath,
                    optional: false,
                    reloadOnChange: false);
            }
        }

        return builder.Build();
    }

    private static IEnumerable<string> GetSettingsPaths() =>
        [
            "appsettings.Acceptance.json",
            "src/Apps/HostedServices/appsettings.Acceptance.json",
            "src/Apps/Web/appsettings.Acceptance.json",
            "appsettings.Acceptance.local.json"
        ];

    private static string ReadEnvironmentValue(string variableName)
    {
        return Environment.GetEnvironmentVariable(variable: variableName)
            ?? Environment.GetEnvironmentVariable(
                variable: variableName,
                target: EnvironmentVariableTarget.User)
            ?? Environment.GetEnvironmentVariable(
                variable: variableName,
                target: EnvironmentVariableTarget.Machine);
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo directory = new(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (Directory.Exists(path: Path.Combine(
                    path1: directory.FullName,
                    path2: ".git"))
                || File.Exists(path: Path.Combine(
                    path1: directory.FullName,
                    path2: ".git")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        return Directory.GetCurrentDirectory();
    }
}