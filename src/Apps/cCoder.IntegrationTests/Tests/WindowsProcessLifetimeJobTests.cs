// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Diagnostics;
using cCoder.IntegrationTests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace cCoder.IntegrationTests.Tests;

public sealed partial class WindowsProcessLifetimeJobTests
{
    [Fact]
    public async Task DisposeShouldTerminateAssignedProcess()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        // Given
        using Process process = Process.Start(
            startInfo: new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/d /s /c \"ping -t 127.0.0.1 > nul\"",
                CreateNoWindow = true,
                UseShellExecute = false
            });

        using WindowsProcessLifetimeJob processLifetimeJob = new();
        processLifetimeJob.Add(process: process);

        // When
        processLifetimeJob.Dispose();

        using CancellationTokenSource cancellationTokenSource = new(
            delay: TimeSpan.FromSeconds(seconds: 5));

        await process.WaitForExitAsync(cancellationToken: cancellationTokenSource.Token);

        // Then
        process.HasExited.Should()
            .BeTrue();
    }
}