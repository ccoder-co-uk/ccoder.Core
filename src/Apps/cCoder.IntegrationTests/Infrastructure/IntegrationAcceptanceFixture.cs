using System.Diagnostics;
using System.Net;
using System.Text;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using cCoder.IntegrationTests.Models;
using Xunit;

namespace cCoder.IntegrationTests.Infrastructure;

public sealed class IntegrationAcceptanceFixture : IAsyncLifetime
{
    private const string DecryptionKey = "000000000000000000000000000000000000000000000000";
    private readonly HttpClientHandler insecureHttpHandler = new()
    {
        AutomaticDecompression = DecompressionMethods.All,
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    };

    private IntegrationAcceptanceDatabaseManager databaseManager;
    private ServiceProvider databaseServices;
    private ExternalProcessApplication webApplication;
    private ExternalProcessApplication hostedServicesApplication;
    private ExternalProcessApplication workflowApplication;
    private readonly string repositoryRoot = FindRepositoryRoot();

    internal AcceptanceSettings Settings { get; private set; }

    public IServiceProvider DatabaseServices => databaseServices;

    public Uri WebBaseAddress { get; private set; }

    public Uri HostedServicesBaseAddress { get; private set; }

    public Uri WorkflowBaseAddress { get; private set; }

    public HttpClient WebClient { get; private set; }

    public HttpClient HostedServicesClient { get; private set; }

    public string WebOutput => webApplication?.Output ?? string.Empty;

    public string HostedServicesOutput => hostedServicesApplication?.Output ?? string.Empty;

    public string WorkflowOutput => workflowApplication?.Output ?? string.Empty;

    public async Task InitializeAsync()
    {
        Settings = new AcceptanceSettings
        {
            CoreConnectionString = AddDatabaseSuffix("CCODER_ACCEPTANCE_CORE_CONNECTION_STRING"),
            SsoConnectionString = AddDatabaseSuffix("CCODER_ACCEPTANCE_SSO_CONNECTION_STRING"),
            DecryptionKey = DecryptionKey
        };

        int webHttpsPort = FindFreePort();
        int hostedServicesHttpPort = FindFreePort();
        int workflowHttpPort = FindFreePort();
        string workflowOutputDirectory = Path.Combine(
            repositoryRoot,
            "src",
            "Apps",
            "Workflow",
            "bin",
            "Debug",
            "net10.0");

        WebBaseAddress = new Uri($"https://localhost:{webHttpsPort}/");
        HostedServicesBaseAddress = new Uri($"http://localhost:{hostedServicesHttpPort}/");
        WorkflowBaseAddress = new Uri($"http://localhost:{workflowHttpPort}/api/");

        databaseServices = IntegrationServiceProviderFactory.Create(Settings);
        Console.WriteLine("Integration fixture: database service provider created.");
        databaseManager = new IntegrationAcceptanceDatabaseManager(databaseServices);
        await databaseManager.ResetDatabasesAsync();
        Console.WriteLine("Integration fixture: acceptance databases reset.");

        await new IntegrationAcceptanceSeeder(databaseServices).SeedAsync();
        Console.WriteLine("Integration fixture: baseline data seeded.");

        await BuildApplicationAsync(
            "src\\Apps\\Workflow\\Workflow.csproj",
            string.Empty);
        Console.WriteLine("Integration fixture: Workflow built.");

        await BuildApplicationAsync(
            "src\\Apps\\HostedServices\\HostedServices.csproj",
            string.Empty);
        Console.WriteLine("Integration fixture: HostedServices built.");

        await BuildApplicationAsync(
            "src\\Apps\\Web\\Web.csproj",
            string.Empty);
        Console.WriteLine("Integration fixture: Web built.");

        workflowApplication = new ExternalProcessApplication("Workflow");
        await workflowApplication.StartAsync(
            ResolveFuncExecutablePath(),
            $"start --port {workflowHttpPort} --csharp --no-build",
            workflowOutputDirectory,
            new Dictionary<string, string>
            {
                ["FUNCTIONS_WORKER_RUNTIME"] = "dotnet-isolated"
            },
            readinessProbe: () => ProbeServerAsync(WorkflowBaseAddress),
            timeout: TimeSpan.FromMinutes(2));
        Console.WriteLine("Integration fixture: Workflow started.");

        await StartHostedServicesAsync();

        webApplication = new ExternalProcessApplication("Web");
        await webApplication.StartAsync(
            "dotnet",
            "run --no-build --no-launch-profile --project src\\Apps\\Web\\Web.csproj",
            repositoryRoot,
            new Dictionary<string, string>
            {
                ["ASPNETCORE_ENVIRONMENT"] = "Acceptance",
                ["ASPNETCORE_URLS"] = WebBaseAddress.ToString(),
                ["ConnectionStrings__Core"] = Settings.CoreConnectionString,
                ["ConnectionStrings__SSO"] = Settings.SsoConnectionString,
                ["Settings__DecryptionKey"] = Settings.DecryptionKey,
                ["Settings__sslPort"] = webHttpsPort.ToString(),
                ["Settings__enableExternalEventing"] = "true",
                ["Services__Workflow"] = WorkflowBaseAddress.ToString(),
                ["Services__HostedServices"] = HostedServicesBaseAddress.ToString(),
                ["Eventing__Http__MaxConcurrency"] = "1"
            },
            readinessProbe: () => ProbeAsync(new Uri(WebBaseAddress, "Api/Time"), useInsecureHandler: true),
            timeout: TimeSpan.FromMinutes(2));
        Console.WriteLine("Integration fixture: Web started.");

        WebClient = CreateClient(WebBaseAddress, useInsecureHandler: true);
        HostedServicesClient = CreateClient(HostedServicesBaseAddress, useInsecureHandler: false);
    }

    public async Task RestartHostedServicesAsync()
    {
        if (hostedServicesApplication is not null)
            await hostedServicesApplication.DisposeAsync();

        await StartHostedServicesAsync();
    }

    public async Task DisposeAsync()
    {
        WebClient?.Dispose();
        HostedServicesClient?.Dispose();

        if (webApplication is not null)
            await webApplication.DisposeAsync();

        if (hostedServicesApplication is not null)
            await hostedServicesApplication.DisposeAsync();

        if (workflowApplication is not null)
            await workflowApplication.DisposeAsync();

        if (databaseManager is not null)
            await databaseManager.DropDatabasesAsync();

        if (databaseServices is not null)
            await databaseServices.DisposeAsync();
    }

    private static int FindFreePort()
    {
        using System.Net.Sockets.TcpListener listener = new(IPAddress.Loopback, 0);
        listener.Start();
        return ((IPEndPoint)listener.LocalEndpoint).Port;
    }

    private HttpClient CreateClient(Uri baseAddress, bool useInsecureHandler)
    {
        HttpClient client = useInsecureHandler
            ? new HttpClient(insecureHttpHandler, disposeHandler: false)
            : new HttpClient();

        client.BaseAddress = baseAddress;
        client.Timeout = TimeSpan.FromMinutes(2);
        return client;
    }

    private async Task<bool> ProbeAsync(Uri uri, bool useInsecureHandler = false)
    {
        using HttpClient client = CreateClient(new Uri($"{uri.Scheme}://{uri.Authority}/"), useInsecureHandler);

        try
        {
            using HttpResponseMessage response = await client.GetAsync(uri.PathAndQuery);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private async Task BuildApplicationAsync(string projectPath, string msbuildProperties)
    {
        string localBuildProperties = ResolveLocalBuildProperties();
        string combinedProperties = CombineMsBuildProperties(localBuildProperties, msbuildProperties);

        await RunCommandAsync(
            "dotnet",
            $"restore {projectPath} {combinedProperties}");

        await RunCommandAsync(
            "dotnet",
            $"build {projectPath} --no-restore -p:UseSharedCompilation=false {combinedProperties}");
    }

    private string ResolveLocalBuildProperties()
    {
        string localSchedulingProject = Path.GetFullPath(
            Path.Combine(
                repositoryRoot,
                "..",
                "cCoder.Scheduling",
                "src",
                "cCoder.Scheduling",
                "cCoder.Scheduling.csproj"));

        return File.Exists(localSchedulingProject)
            ? "-p:UseLocalScheduling=true -p:GenerateAssemblyInfo=false -p:GenerateTargetFrameworkAttribute=false"
            : string.Empty;
    }

    private static string CombineMsBuildProperties(params string[] values) =>
        string.Join(" ", values.Where(value => !string.IsNullOrWhiteSpace(value)));

    private async Task RunCommandAsync(string fileName, string arguments)
    {
        StringBuilder output = new();

        using Process process = new()
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                WorkingDirectory = repositoryRoot,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            },
            EnableRaisingEvents = true
        };

        process.OutputDataReceived += (_, args) =>
        {
            if (args.Data is not null)
                output.AppendLine(args.Data);
        };

        process.ErrorDataReceived += (_, args) =>
        {
            if (args.Data is not null)
                output.AppendLine(args.Data);
        };

        if (!process.Start())
            throw new InvalidOperationException($"Failed to start command '{fileName} {arguments}'.");

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
            throw new InvalidOperationException(
                $"Command '{fileName} {arguments}' failed with exit code {process.ExitCode}.{Environment.NewLine}{output}");
    }

    private static async Task<bool> ProbeServerAsync(Uri baseAddress)
    {
        using HttpClient client = new()
        {
            BaseAddress = new Uri($"{baseAddress.Scheme}://{baseAddress.Authority}/"),
            Timeout = TimeSpan.FromSeconds(5)
        };

        try
        {
            using HttpResponseMessage response = await client.GetAsync(string.Empty);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static string AddDatabaseSuffix(string variableName)
    {
        string connectionString = ReadRequiredConnectionString(variableName);

        SqlConnectionStringBuilder builder = new(connectionString)
        {
            Encrypt = true,
            TrustServerCertificate = true
        };

        string databaseName = builder.InitialCatalog ?? string.Empty;
        if (string.IsNullOrWhiteSpace(databaseName))
            return connectionString;

        builder.InitialCatalog = $"{databaseName}-ccoder-integrationtests";
        return builder.ConnectionString;
    }

    private static string ReadRequiredConnectionString(string variableName)
    {
        string connectionString =
            Environment.GetEnvironmentVariable(variableName)
            ?? Environment.GetEnvironmentVariable(variableName, EnvironmentVariableTarget.User)
            ?? Environment.GetEnvironmentVariable(variableName, EnvironmentVariableTarget.Machine);

        if (!string.IsNullOrWhiteSpace(connectionString))
            return connectionString;

        throw new InvalidOperationException(
            $"Acceptance connection string environment variable '{variableName}' was not found.");
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo directory = new(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "src", "cCoder.Core.sln")))
                return directory.FullName;

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Could not locate the ccoder.Core repository root.");
    }

    private async Task StartHostedServicesAsync()
    {
        hostedServicesApplication = new ExternalProcessApplication("HostedServices");
        await hostedServicesApplication.StartAsync(
            "dotnet",
            "run --no-build --no-launch-profile --project src\\Apps\\HostedServices\\HostedServices.csproj",
            repositoryRoot,
            new Dictionary<string, string>
            {
                ["ASPNETCORE_ENVIRONMENT"] = "Acceptance",
                ["ASPNETCORE_URLS"] = HostedServicesBaseAddress.ToString(),
                ["ConnectionStrings__Core"] = Settings.CoreConnectionString,
                ["ConnectionStrings__SSO"] = Settings.SsoConnectionString,
                ["Settings__DecryptionKey"] = Settings.DecryptionKey,
                ["Settings__sslPort"] = WebBaseAddress.Port.ToString(),
                ["Services__Workflow"] = WorkflowBaseAddress.ToString()
            },
            readinessProbe: () => ProbeAsync(new Uri(HostedServicesBaseAddress, "Workflow/GetStats")),
            timeout: TimeSpan.FromMinutes(2));
        Console.WriteLine("Integration fixture: HostedServices started.");
    }

    private static string ResolveFuncExecutablePath()
    {
        string bundledFuncExe = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "npm",
            "node_modules",
            "azure-functions-core-tools",
            "bin",
            "in-proc6",
            "func.exe");

        if (File.Exists(bundledFuncExe))
            return bundledFuncExe;

        string fallbackFuncExe = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "npm",
            "node_modules",
            "azure-functions-core-tools",
            "bin",
            "func.exe");

        if (File.Exists(fallbackFuncExe))
            return fallbackFuncExe;

        string roamingNpmFunc = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "npm",
            "func.cmd");

        if (File.Exists(roamingNpmFunc))
            return roamingNpmFunc;

        return "func";
    }
}

[CollectionDefinition(Name)]
public sealed class IntegrationAcceptanceCollection
    : ICollectionFixture<IntegrationAcceptanceFixture>
{
    public const string Name = "Integration acceptance";
}
