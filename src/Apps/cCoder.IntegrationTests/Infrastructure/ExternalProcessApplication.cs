using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace cCoder.IntegrationTests.Infrastructure;

internal sealed class ExternalProcessApplication : IAsyncDisposable
{
    private readonly StringBuilder output = new();
    private Process process;

    public string Name { get; }

    public ExternalProcessApplication(string name) => Name = name;

    public string Output => output.ToString();

    public async Task StartAsync(
        string fileName,
        string arguments,
        string workingDirectory,
        IReadOnlyDictionary<string, string> environmentVariables,
        Func<Task<bool>> readinessProbe,
        TimeSpan timeout)
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
            process.StartInfo.Environment[key] = value;

        process.OutputDataReceived += (_, args) => Append(args.Data);
        process.ErrorDataReceived += (_, args) => Append(args.Data);

        if (!process.Start())
            throw new InvalidOperationException($"Failed to start process '{Name}'.");

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        using CancellationTokenSource cancellationTokenSource = new(timeout);

        while (!cancellationTokenSource.IsCancellationRequested)
        {
            if (process.HasExited)
                throw new InvalidOperationException($"Process '{Name}' exited before it became ready.{Environment.NewLine}{Output}");

            if (await readinessProbe())
                return;

            await Task.Delay(500, cancellationTokenSource.Token).ContinueWith(_ => { }, TaskScheduler.Default);
        }

        throw new TimeoutException($"Process '{Name}' did not become ready within {timeout}.{Environment.NewLine}{Output}");
    }

    public async ValueTask DisposeAsync()
    {
        if (process is null)
            return;

        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
                await process.WaitForExitAsync();
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
            return;

        lock (output)
            output.AppendLine(line);
    }
}
