// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace cCoder.IntegrationTests.Infrastructure;

internal sealed class ExternalProcessApplication(string name) : IAsyncDisposable
{
    private readonly StringBuilder output = new();
    private Process process;

    public string Name { get; } = name;

    public string Output => output.ToString();

    public async Task StartAsync(
        string fileName,
        string arguments,
        string workingDirectory,
        IReadOnlyDictionary<string, string> environmentVariables,
        Func<Task<bool>> readinessProbe,
        TimeSpan timeout,
        Func<string> readinessDiagnostics = null)
    {
        process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                WorkingDirectory = workingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            },
            EnableRaisingEvents = true
        };

        foreach ((string key, string value) in environmentVariables)
        {
            process.StartInfo.Environment[key] = value;
        }

        process.OutputDataReceived += (_, args) => Append(line: args.Data);
        process.ErrorDataReceived += (_, args) => Append(line: args.Data);

        if (!process.Start())
        {
            throw new InvalidOperationException($"Failed to start process '{Name}'.");
        }

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        using CancellationTokenSource cancellationTokenSource = new(timeout);

        while (!cancellationTokenSource.IsCancellationRequested)
        {
            if (process.HasExited)
            {
                throw new InvalidOperationException($"Process '{Name}' exited before it became ready.{Environment.NewLine}{Output}");
            }

            if (await readinessProbe())
            {
                return;
            }

            await Task.Delay(millisecondsDelay: 500,cancellationToken: cancellationTokenSource.Token)
                .ContinueWith(continuationAction: _ => { },scheduler: TaskScheduler.Default);
        }

        string diagnostics = readinessDiagnostics?.Invoke();

        string readinessDetails = string.IsNullOrWhiteSpace(value: diagnostics)
            ? string.Empty
            : $"{Environment.NewLine}Readiness diagnostics:{Environment.NewLine}{diagnostics}";

        throw new TimeoutException(
            $"Process '{Name}' did not become ready within {timeout}.{readinessDetails}{Environment.NewLine}{Output}");
    }

    public async ValueTask DisposeAsync()
    {
        if (process is null)
        {
            return;
        }

        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);

                Task waitForExitTask = process.WaitForExitAsync();
                Task completedTask = await Task.WhenAny(task1: waitForExitTask,task2: Task.Delay(delay: TimeSpan.FromSeconds(seconds: 15)));

                if (completedTask == waitForExitTask)
                {
                    await waitForExitTask;
                }
            }
        }
        catch
        {
            // ignore cleanup failures
        }
        finally
        {
            process.Dispose();
        }
    }

    private void Append(string line)
    {
        if (line is null)
        {
            return;
        }

        lock (output)
        {
            output.AppendLine(value: line);
        }
    }
}