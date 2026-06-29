using System.Diagnostics;
using System.Net;
using System.Text;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using cCoder.IntegrationTests.Models;
using Xunit;

namespace cCoder.IntegrationTests.Infrastructure;

public sealed class IntegrationAcceptanceFixture : IAsyncLifetime
{
    private const string DecryptionKey = "000000000000000000000000000000000000000000000000";
    private static readonly string[] ServiceBusEventQueues =
    [
        "app_add",
        "app_update",
        "app_delete",
        "folder_delete",
        "flow_instance_data_add"
    ];

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
    private string acceptanceArtifactsRoot;
    private string workflowOutputDirectory;
    private string hostedServicesOutputDirectory;
    private string webOutputDirectory;

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
            DecryptionKey = DecryptionKey,
            EventProviderType = ResolveEventProviderType(),
            ServiceBusConnectionString = ResolveOptionalSetting(
                "CCODER_INTEGRATION_SERVICE_BUS_CONNECTION_STRING",
                "ConnectionStrings__ServiceBus",
                "EVENT_LIBRARY_AZURE_SERVICE_BUS_CONNECTION_STRING"),
            ServiceBusMaxConcurrency = ResolveIntSetting(
                "CCODER_INTEGRATION_SERVICE_BUS_MAX_CONCURRENCY",
                "Eventing__ServiceBus__MaxConcurrency",
                1)
        };

        if (Settings.UseServiceBusEventing)
            await EnsureServiceBusQueuesAreCleanAsync();

        int webHttpsPort = FindFreePort();
        int hostedServicesHttpPort = FindFreePort();
        int workflowHttpPort = FindFreePort();
        acceptanceArtifactsRoot = Path.Combine(
            repositoryRoot,
            "artifacts",
            "integration-tests",
            Guid.NewGuid().ToString("N"));
        workflowOutputDirectory = Path.Combine(acceptanceArtifactsRoot, "Workflow");
        hostedServicesOutputDirectory = Path.Combine(acceptanceArtifactsRoot, "HostedServices");
        webOutputDirectory = Path.Combine(acceptanceArtifactsRoot, "Web");

        Directory.CreateDirectory(workflowOutputDirectory);
        Directory.CreateDirectory(hostedServicesOutputDirectory);
        Directory.CreateDirectory(webOutputDirectory);

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
            string.Empty,
            workflowOutputDirectory,
            Path.Combine(acceptanceArtifactsRoot, "obj", "Workflow"));
        Console.WriteLine("Integration fixture: Workflow built.");

        await BuildApplicationAsync(
            "src\\Apps\\HostedServices\\HostedServices.csproj",
            string.Empty,
            hostedServicesOutputDirectory,
            Path.Combine(acceptanceArtifactsRoot, "obj", "HostedServices"));
        Console.WriteLine("Integration fixture: HostedServices built.");

        await BuildApplicationAsync(
            "src\\Apps\\Web\\Web.csproj",
            string.Empty,
            webOutputDirectory,
            Path.Combine(acceptanceArtifactsRoot, "obj", "Web"));
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

        Dictionary<string, string> webEnvironment = CreateCommonApplicationEnvironment();
        webEnvironment["ASPNETCORE_URLS"] = WebBaseAddress.ToString();
        webEnvironment["Settings__sslPort"] = webHttpsPort.ToString();
        webEnvironment["Settings__enableExternalEventing"] = "true";
        webEnvironment["Services__HostedServices"] = HostedServicesBaseAddress.ToString();

        webApplication = new ExternalProcessApplication("Web");
        await webApplication.StartAsync(
            "dotnet",
            $"\"{Path.Combine(webOutputDirectory, "Web.dll")}\"",
            webOutputDirectory,
            webEnvironment,
            readinessProbe: async () =>
                await ProbeAsync(new Uri(WebBaseAddress, "Api/Time"), useInsecureHandler: true)
                || HasApplicationStarted(webApplication),
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

        try
        {
            if (Settings?.UseServiceBusEventing == true)
                await DrainServiceBusQueuesAsync();

            if (!string.IsNullOrWhiteSpace(acceptanceArtifactsRoot) && Directory.Exists(acceptanceArtifactsRoot))
                Directory.Delete(acceptanceArtifactsRoot, recursive: true);
        }
        catch
        {
            // Best-effort cleanup only. A failed delete should not hide the test outcome.
        }
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

    private static bool HasApplicationStarted(ExternalProcessApplication application) =>
        application.Output.Contains("Application started.", StringComparison.Ordinal);

    private async Task BuildApplicationAsync(
        string projectPath,
        string msbuildProperties,
        string outputDirectory,
        string intermediateDirectory)
    {
        string localBuildProperties = ResolveLocalBuildProperties();
        string outputProperties =
            $"-p:OutputPath=\"{FormatMsBuildPath(outputDirectory, trailingSlash: false)}\" " +
            $"-p:IntermediateOutputPath=\"{FormatMsBuildPath(intermediateDirectory, trailingSlash: true)}\"";
        string combinedProperties = CombineMsBuildProperties(localBuildProperties, msbuildProperties, outputProperties);

        await RunCommandAsync(
            "dotnet",
            $"restore {projectPath} {combinedProperties}");

        await RunCommandAsync(
            "dotnet",
            $"build {projectPath} --no-restore -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false {combinedProperties}");
    }

    private string ResolveLocalBuildProperties()
    {
        bool useLocalScheduling = string.Equals(
            Environment.GetEnvironmentVariable("CCODER_INTEGRATION_USE_LOCAL_SCHEDULING"),
            "true",
            StringComparison.OrdinalIgnoreCase);
        bool useLocalWorkflow = string.Equals(
            Environment.GetEnvironmentVariable("CCODER_INTEGRATION_USE_LOCAL_WORKFLOW"),
            "true",
            StringComparison.OrdinalIgnoreCase);

        if (!useLocalScheduling && !useLocalWorkflow)
            return string.Empty;

        string localSchedulingProject = Path.GetFullPath(
            Path.Combine(
                repositoryRoot,
                "..",
                "cCoder.Scheduling",
                "src",
                "cCoder.Scheduling",
                "cCoder.Scheduling.csproj"));
        string localWorkflowProject = Path.GetFullPath(
            Path.Combine(
                repositoryRoot,
                "..",
                "cCoder.Workflow",
                "src",
                "cCoder.Workflow",
                "cCoder.Workflow.csproj"));

        List<string> properties = [];

        if (useLocalScheduling && File.Exists(localSchedulingProject))
            properties.Add("-p:UseLocalScheduling=true");

        if (useLocalWorkflow && File.Exists(localWorkflowProject))
            properties.Add("-p:UseLocalWorkflow=true");

        if (properties.Count > 0)
        {
            properties.Add("-p:GenerateAssemblyInfo=false");
            properties.Add("-p:GenerateTargetFrameworkAttribute=false");
        }

        return string.Join(" ", properties);
    }

    private static string CombineMsBuildProperties(params string[] values) =>
        string.Join(" ", values.Where(value => !string.IsNullOrWhiteSpace(value)));

    private static string FormatMsBuildPath(string path, bool trailingSlash)
    {
        string formattedPath = path.Replace('\\', '/');

        if (trailingSlash && !formattedPath.EndsWith('/'))
            formattedPath += '/';

        return formattedPath;
    }

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
        Dictionary<string, string> hostedServicesEnvironment = CreateCommonApplicationEnvironment();
        hostedServicesEnvironment["ASPNETCORE_URLS"] = HostedServicesBaseAddress.ToString();
        hostedServicesEnvironment["Settings__sslPort"] = WebBaseAddress.Port.ToString();

        hostedServicesApplication = new ExternalProcessApplication("HostedServices");
        await hostedServicesApplication.StartAsync(
            "dotnet",
            $"\"{Path.Combine(hostedServicesOutputDirectory, "HostedServices.dll")}\"",
            hostedServicesOutputDirectory,
            hostedServicesEnvironment,
            readinessProbe: () => ProbeAsync(new Uri(HostedServicesBaseAddress, "Workflow/GetStats")),
            timeout: TimeSpan.FromMinutes(2));
        Console.WriteLine("Integration fixture: HostedServices started.");
    }

    private Dictionary<string, string> CreateCommonApplicationEnvironment()
    {
        Dictionary<string, string> environment = new()
        {
            ["ASPNETCORE_ENVIRONMENT"] = "Acceptance",
            ["ConnectionStrings__Core"] = Settings.CoreConnectionString,
            ["ConnectionStrings__SSO"] = Settings.SsoConnectionString,
            ["Settings__DecryptionKey"] = Settings.DecryptionKey,
            ["Services__Workflow"] = WorkflowBaseAddress.ToString(),
            ["Eventing__ProviderType"] = Settings.EventProviderType,
            ["Eventing__Http__MaxConcurrency"] = "1"
        };

        if (Settings.UseServiceBusEventing)
        {
            environment["ConnectionStrings__ServiceBus"] = Settings.ServiceBusConnectionString;
            environment["Eventing__ServiceBus__MaxConcurrency"] =
                Settings.ServiceBusMaxConcurrency.ToString();
        }

        return environment;
    }

    private async Task EnsureServiceBusQueuesAreCleanAsync()
    {
        if (string.IsNullOrWhiteSpace(Settings.ServiceBusConnectionString))
        {
            throw new InvalidOperationException(
                "Service Bus integration mode requires CCODER_INTEGRATION_SERVICE_BUS_CONNECTION_STRING or ConnectionStrings__ServiceBus.");
        }

        ServiceBusAdministrationClient administrationClient = new(Settings.ServiceBusConnectionString);

        foreach (string queueName in ServiceBusEventQueues)
        {
            if (!await administrationClient.QueueExistsAsync(queueName))
                await administrationClient.CreateQueueAsync(queueName);
        }

        await DrainServiceBusQueuesAsync();
    }

    private async Task DrainServiceBusQueuesAsync()
    {
        if (string.IsNullOrWhiteSpace(Settings?.ServiceBusConnectionString))
            return;

        await using ServiceBusClient client = new(Settings.ServiceBusConnectionString);

        foreach (string queueName in ServiceBusEventQueues)
        {
            ServiceBusReceiver receiver = client.CreateReceiver(queueName);

            while (true)
            {
                IReadOnlyList<ServiceBusReceivedMessage> messages =
                    await receiver.ReceiveMessagesAsync(100, TimeSpan.FromSeconds(1));

                if (messages.Count == 0)
                    break;

                foreach (ServiceBusReceivedMessage message in messages)
                    await receiver.CompleteMessageAsync(message);
            }

            await receiver.DisposeAsync();
        }
    }

    private static string ResolveEventProviderType() =>
        ResolveOptionalSetting("CCODER_INTEGRATION_EVENT_PROVIDER", "Eventing__ProviderType")
        ?? "Http";

    private static int ResolveIntSetting(
        string primaryName,
        string secondaryName,
        int fallback)
    {
        string raw = ResolveOptionalSetting(primaryName, secondaryName);

        return int.TryParse(raw, out int value)
            ? value
            : fallback;
    }

    private static string ResolveOptionalSetting(params string[] variableNames)
    {
        foreach (string variableName in variableNames)
        {
            string value =
                Environment.GetEnvironmentVariable(variableName)
                ?? Environment.GetEnvironmentVariable(variableName, EnvironmentVariableTarget.User)
                ?? Environment.GetEnvironmentVariable(variableName, EnvironmentVariableTarget.Machine);

            if (!string.IsNullOrWhiteSpace(value))
                return value;
        }

        return null;
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
