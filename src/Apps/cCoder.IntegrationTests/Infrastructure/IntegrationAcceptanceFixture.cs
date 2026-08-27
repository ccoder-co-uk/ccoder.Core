// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Diagnostics;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using cCoder.Security.Data.EF.Interfaces;
using cCoder.Security.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.WebUtilities;
using cCoder.Core.Testing;
using cCoder.IntegrationTests.Models;
using Xunit;

namespace cCoder.IntegrationTests.Infrastructure;

public sealed class IntegrationAcceptanceFixture : IAsyncLifetime
{
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
    private string lastHealthProbeFailure;

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

    internal async Task<string> CreateAuthTokenAsync(string userId)
    {
        string selector = WebEncoders.Base64UrlEncode(
            input: RandomNumberGenerator.GetBytes(count: 16));

        string secret = WebEncoders.Base64UrlEncode(
            input: RandomNumberGenerator.GetBytes(count: 32));

        await using DbContext sso = DatabaseServices
            .GetRequiredService<ISecurityDbContextFactory>()
            .CreateDbContext(ignoreAuthInfo: true);

        sso.Add(entity: new Token
        {
            Id = selector,
            SecretHash = Convert.ToHexString(
                inArray: SHA256.HashData(
                    source: Encoding.UTF8.GetBytes(s: secret))),
            Reason = (int)TokenUse.Auth,
            Expires = DateTimeOffset.UtcNow.AddHours(hours: 1),
            UserName = userId
        });

        await sso.SaveChangesAsync();
        return $"{selector}.{secret}";
    }

    public async Task InitializeAsync()
    {
        IntegrationTestConfiguration configuration =
            IntegrationTestConfiguration.Load();

        Settings = new AcceptanceSettings
        {
            CoreConnectionString =
                configuration.Acceptance.CoreConnectionString,
            SsoConnectionString =
                configuration.Acceptance.SecurityConnectionString,
            DecryptionKey =
                configuration.Acceptance.DecryptionKey,
            EventProviderType = configuration.EventProviderType,
            ServiceBusConnectionString =
                configuration.ServiceBusConnectionString,
            ServiceBusMaxConcurrency =
                configuration.ServiceBusMaxConcurrency,
            MailTenantId = configuration.MailTenantId,
            MailClientId = configuration.MailClientId,
            MailClientSecret = configuration.MailClientSecret,
            MailSendUser = configuration.MailSendUser,
            MailReceiveUser = configuration.MailReceiveUser,
            KeepArtifacts = configuration.KeepArtifacts,
            UseLocalWorkflow = configuration.UseLocalWorkflow,
            UseLocalSecurity = configuration.UseLocalSecurity,
            UseLocalAppSecurity =
                configuration.UseLocalAppSecurity,
            UseLocalData = configuration.UseLocalData,
            UseLocalContentManagement =
                configuration.UseLocalContentManagement,
            LocalContentManagementProject =
                configuration.LocalContentManagementProject,
            LocalSecurityAssemblyVersion =
                configuration.LocalSecurityAssemblyVersion
        };

        if (Settings.UseServiceBusEventing)
        {
            await EnsureServiceBusQueuesAreCleanAsync();
        }

        int webHttpsPort = FindFreePort();
        int hostedServicesHttpPort = FindFreePort();
        int workflowHttpPort = FindFreePort();

        acceptanceArtifactsRoot = Path.Combine(
path1: repositoryRoot, path2: "artifacts", path3: "integration-tests", path4: Guid.NewGuid()
            .ToString(format: "N"));

        workflowOutputDirectory = Path.Combine(path1: acceptanceArtifactsRoot, path2: "Workflow");
        hostedServicesOutputDirectory = Path.Combine(path1: acceptanceArtifactsRoot, path2: "HostedServices");
        webOutputDirectory = Path.Combine(path1: acceptanceArtifactsRoot, path2: "Web");

        Directory.CreateDirectory(path: workflowOutputDirectory);
        Directory.CreateDirectory(path: hostedServicesOutputDirectory);
        Directory.CreateDirectory(path: webOutputDirectory);

        WebBaseAddress = new Uri($"https://localhost:{webHttpsPort}/");
        HostedServicesBaseAddress = new Uri($"http://localhost:{hostedServicesHttpPort}/");
        WorkflowBaseAddress = new Uri($"http://localhost:{workflowHttpPort}/api/");

        databaseServices = IntegrationServiceProviderFactory.Create(settings: Settings);
        Console.WriteLine(value: "Integration fixture: database service provider created.");

        databaseManager = new IntegrationAcceptanceDatabaseManager(
            databaseServices,
            Settings.CoreConnectionString,
            Settings.SsoConnectionString);

        await databaseManager.ResetDatabasesAsync();
        Console.WriteLine(value: "Integration fixture: acceptance databases reset.");

        await new IntegrationAcceptanceSeeder(databaseServices).SeedAsync();
        Console.WriteLine(value: "Integration fixture: baseline data seeded.");

        await BuildApplicationAsync(
projectPath: "src\\Apps\\Workflow\\Workflow.csproj", msbuildProperties: string.Empty, outputDirectory: workflowOutputDirectory, intermediateDirectory: Path.Combine(path1: acceptanceArtifactsRoot, path2: "obj", path3: "Workflow"));

        Console.WriteLine(value: "Integration fixture: Workflow built.");

        await BuildApplicationAsync(
projectPath: "src\\Apps\\HostedServices\\HostedServices.csproj", msbuildProperties: string.Empty, outputDirectory: hostedServicesOutputDirectory, intermediateDirectory: Path.Combine(path1: acceptanceArtifactsRoot, path2: "obj", path3: "HostedServices"));

        Console.WriteLine(value: "Integration fixture: HostedServices built.");

        await BuildApplicationAsync(
projectPath: "src\\Apps\\Web\\Web.csproj", msbuildProperties: string.Empty, outputDirectory: webOutputDirectory, intermediateDirectory: Path.Combine(path1: acceptanceArtifactsRoot, path2: "obj", path3: "Web"));

        Console.WriteLine(value: "Integration fixture: Web built.");

        workflowApplication = new ExternalProcessApplication("Workflow");

        await workflowApplication.StartAsync(
fileName: ResolveFuncExecutablePath(), arguments: $"start --port {workflowHttpPort} --csharp --no-build", workingDirectory: workflowOutputDirectory, environmentVariables: new Dictionary<string, string>
{
    ["FUNCTIONS_WORKER_RUNTIME"] = "dotnet-isolated"
}, readinessProbe: () => ProbeHealthAsync(baseAddress: WorkflowBaseAddress), timeout: TimeSpan.FromMinutes(minutes: 2), readinessDiagnostics: GetHealthProbeDiagnostics);

        Console.WriteLine(value: "Integration fixture: Workflow started.");

        await StartHostedServicesAsync();

        Dictionary<string, string> webEnvironment = CreateCommonApplicationEnvironment();
        AddHttpsCertificateEnvironment(environment: webEnvironment);
        webEnvironment["ASPNETCORE_URLS"] = WebBaseAddress.ToString();

        webEnvironment["Eventing__Http__HubUrl"] =
            new Uri(
                baseUri: HostedServicesBaseAddress,
                relativeUri: "Api/Eventing")
            .ToString();

        webApplication = new ExternalProcessApplication("Web");

        await webApplication.StartAsync(
fileName: "dotnet", arguments: $"\"{Path.Combine(path1: webOutputDirectory, path2: "Web.dll")}\"", workingDirectory: webOutputDirectory, environmentVariables: webEnvironment, readinessProbe: () => ProbeHealthAsync(baseAddress: WebBaseAddress, useInsecureHandler: true), timeout: TimeSpan.FromMinutes(minutes: 2), readinessDiagnostics: GetHealthProbeDiagnostics);

        Console.WriteLine(value: "Integration fixture: Web started.");

        WebClient = CreateClient(baseAddress: WebBaseAddress, useInsecureHandler: true);
        HostedServicesClient = CreateClient(baseAddress: HostedServicesBaseAddress, useInsecureHandler: false);
    }

    public async Task RestartHostedServicesAsync()
    {
        if (hostedServicesApplication is not null)
        {
            await hostedServicesApplication.DisposeAsync();
        }

        await StartHostedServicesAsync();
    }

    public async Task DisposeAsync()
    {
        WebClient?.Dispose();
        HostedServicesClient?.Dispose();

        if (webApplication is not null)
        {
            await webApplication.DisposeAsync();
        }

        if (hostedServicesApplication is not null)
        {
            await hostedServicesApplication.DisposeAsync();
        }

        if (workflowApplication is not null)
        {
            await workflowApplication.DisposeAsync();
        }

        if (databaseServices is not null)
        {
            await databaseServices.DisposeAsync();
        }

        if (databaseManager is not null)
        {
            await databaseManager.DropDatabasesAsync();
        }

        try
        {
            if (Settings?.UseServiceBusEventing == true)
            {
                await DrainServiceBusQueuesAsync();
            }

            if (!Settings.KeepArtifacts
                && !string.IsNullOrWhiteSpace(value: acceptanceArtifactsRoot)
                && Directory.Exists(path: acceptanceArtifactsRoot))
            {
                Directory.Delete(path: acceptanceArtifactsRoot, recursive: true);
            }
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
        client.Timeout = TimeSpan.FromMinutes(minutes: 2);
        return client;
    }

    private async Task<bool> ProbeAsync(Uri uri, bool useInsecureHandler = false)
    {
        using HttpClient client = CreateClient(baseAddress: new Uri($"{uri.Scheme}://{uri.Authority}/"), useInsecureHandler: useInsecureHandler);

        try
        {
            using HttpResponseMessage response = await client.GetAsync(requestUri: uri.PathAndQuery);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private async Task BuildApplicationAsync(
        string projectPath,
        string msbuildProperties,
        string outputDirectory,
        string intermediateDirectory)
    {
        string localBuildProperties = ResolveLocalBuildProperties();

        string outputProperties =
            $"-p:OutputPath=\"{FormatMsBuildPath(path: outputDirectory, trailingSlash: false)}\" " +
            $"-p:IntermediateOutputPath=\"{FormatMsBuildPath(path: intermediateDirectory, trailingSlash: true)}\"";

        string combinedProperties = CombineMsBuildProperties(
            values: [localBuildProperties, msbuildProperties, outputProperties]);

        Console.WriteLine(value: $"Integration fixture: building {projectPath} with properties: {combinedProperties}");

        await RunCommandAsync(
fileName: "dotnet", arguments: $"restore {projectPath} {combinedProperties}");

        await RunCommandAsync(
fileName: "dotnet", arguments: $"build {projectPath} --no-restore -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false {combinedProperties}");
    }

    private string ResolveLocalBuildProperties()
    {
        bool useLocalWorkflow = Settings.UseLocalWorkflow;
        bool useLocalSecurity = Settings.UseLocalSecurity;
        bool useLocalAppSecurity = Settings.UseLocalAppSecurity;
        bool useLocalData = Settings.UseLocalData;

        bool useLocalContentManagement =
            Settings.UseLocalContentManagement;

        string localSecurityAssemblyVersion =
            Settings.LocalSecurityAssemblyVersion;

        string localContentManagementProject =
            string.IsNullOrWhiteSpace(
                value: Settings.LocalContentManagementProject)
                ? Path.GetFullPath(
                    path: Path.Combine(
                        paths:
                        [
                            repositoryRoot,
                            "..",
                            "cCoder.ContentManagement",
                            "src",
                            "cCoder.ContentManagement",
                            "cCoder.ContentManagement.csproj"
                        ]))
                : Path.GetFullPath(
                    path: Settings.LocalContentManagementProject);

        string localAppSecurityProject = Path.GetFullPath(
path: Path.Combine(
                paths:
                [
                    repositoryRoot,
                    "..",
                    "cCoder.AppSecurity",
                    "src",
                    "cCoder.AppSecurity",
                    "cCoder.AppSecurity.csproj"
                ]));

        string localDataProject = Path.GetFullPath(
path: Path.Combine(
                paths:
                [
                    repositoryRoot,
                    "..",
                    "cCoder.Data",
                    "src",
                    "cCoder.Data",
                    "cCoder.Data.csproj"
                ]));

        string localWorkflowProject = Path.GetFullPath(
path: Path.Combine(
                paths:
                [
                    repositoryRoot,
                    "..",
                    "cCoder.Workflow",
                    "src",
                    "cCoder.Workflow",
                    "cCoder.Workflow.csproj"
                ]));

        string localSecurityProject = Path.GetFullPath(
path: Path.Combine(
                paths:
                [
                    repositoryRoot,
                    "..",
                    "cCoder.Security",
                    "src",
                    "cCoder.Security",
                    "cCoder.Security.csproj"
                ]));

        if (!useLocalWorkflow
            && !useLocalSecurity
            && !useLocalAppSecurity
            && !useLocalData
            && !useLocalContentManagement)
        {
            return string.Empty;
        }

        List<string> properties = [];

        if (useLocalAppSecurity && File.Exists(path: localAppSecurityProject))
        {
            properties.Add(item: "-p:UseLocalAppSecurity=true");
        }

        if (useLocalData && File.Exists(path: localDataProject))
        {
            properties.Add(item: "-p:UseLocalData=true");
        }

        if (useLocalContentManagement
            && File.Exists(path: localContentManagementProject))
        {
            properties.Add(item: "-p:UseLocalContentManagement=true");

            properties.Add(
                item: $"-p:LocalContentManagementProject=\"{localContentManagementProject}\"");
        }

        if (useLocalSecurity && File.Exists(path: localSecurityProject))
        {
            properties.Add(item: "-p:UseLocalSecurity=true");
        }

        if (useLocalWorkflow && File.Exists(path: localWorkflowProject))
        {
            properties.Add(item: "-p:UseLocalWorkflow=true");
        }

        if (useLocalSecurity && !string.IsNullOrWhiteSpace(value: localSecurityAssemblyVersion))
        {
            properties.Add(item: $"-p:Version={localSecurityAssemblyVersion}");
            properties.Add(item: $"-p:AssemblyVersion={localSecurityAssemblyVersion}");
            properties.Add(item: $"-p:FileVersion={localSecurityAssemblyVersion}");
        }

        return string.Join(separator: " ", values: properties);
    }

    private static string CombineMsBuildProperties(params string[] values) =>
        string.Join(separator: " ", values: values.Where(predicate: value => !string.IsNullOrWhiteSpace(value: value)));

    private static string FormatMsBuildPath(string path, bool trailingSlash)
    {
        string formattedPath = path.Replace(oldChar: '\\', newChar: '/');

        if (trailingSlash && !formattedPath.EndsWith(value: '/'))
        {
            formattedPath += '/';
        }

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

        process.StartInfo.EnvironmentVariables["MSBUILDDISABLENODEREUSE"] = "1";
        process.StartInfo.EnvironmentVariables["DOTNET_CLI_TELEMETRY_OPTOUT"] = "1";

        process.OutputDataReceived += (_, args) =>
        {
            if (args.Data is not null)
            {
                output.AppendLine(value: args.Data);
            }
        };

        process.ErrorDataReceived += (_, args) =>
        {
            if (args.Data is not null)
            {
                output.AppendLine(value: args.Data);
            }
        };

        if (!process.Start())
        {
            throw new InvalidOperationException($"Failed to start command '{fileName} {arguments}'.");
        }

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"Command '{fileName} {arguments}' failed with exit code {process.ExitCode}.{Environment.NewLine}{output}");
        }
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo directory = new(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(path: Path.Combine(path1: directory.FullName, path2: "src", path3: "cCoder.Core.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Could not locate the ccoder.Core repository root.");
    }

    private async Task StartHostedServicesAsync()
    {
        Dictionary<string, string> hostedServicesEnvironment = CreateCommonApplicationEnvironment();
        hostedServicesEnvironment["ASPNETCORE_URLS"] = HostedServicesBaseAddress.ToString();

        hostedServicesApplication = new ExternalProcessApplication("HostedServices");

        await hostedServicesApplication.StartAsync(
fileName: "dotnet", arguments: $"\"{Path.Combine(path1: hostedServicesOutputDirectory, path2: "HostedServices.dll")}\"", workingDirectory: hostedServicesOutputDirectory, environmentVariables: hostedServicesEnvironment, readinessProbe: () => ProbeHealthAsync(baseAddress: HostedServicesBaseAddress), timeout: TimeSpan.FromMinutes(minutes: 2), readinessDiagnostics: GetHealthProbeDiagnostics);

        Console.WriteLine(value: "Integration fixture: HostedServices started.");
    }

    private async Task<bool> ProbeHealthAsync(Uri baseAddress, bool useInsecureHandler = false)
    {
        using HttpClient client = CreateClient(baseAddress: baseAddress, useInsecureHandler: useInsecureHandler);
        Uri healthUri = new(baseAddress, "Health");

        try
        {
            using HttpResponseMessage response = await client.GetAsync(requestUri: "Health");
            string content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode
                && string.Equals(a: content, b: "OK", comparisonType: StringComparison.Ordinal))
            {
                lastHealthProbeFailure = null;
                return true;
            }

            lastHealthProbeFailure =
                $"GET {healthUri} returned {(int)response.StatusCode} {response.StatusCode} with body '{content}'.";

            return false;
        }
        catch (Exception exception)
        {
            lastHealthProbeFailure = $"GET {healthUri} failed: {FormatException(exception: exception)}";
            return false;
        }
    }

    private string GetHealthProbeDiagnostics() =>
        lastHealthProbeFailure ?? "No health probe failure was recorded.";

    private static string FormatException(Exception exception)
    {
        List<string> messages = [];

        for (Exception current = exception; current is not null; current = current.InnerException)
        {
            messages.Add(item: $"{current.GetType().FullName}: {current.Message}");
        }

        return string.Join(separator: " ---> ", values: messages);
    }

    private Dictionary<string, string> CreateCommonApplicationEnvironment()
    {
        Dictionary<string, string> environment = new()
        {
            ["ASPNETCORE_ENVIRONMENT"] = "Acceptance",
            ["CoreData__ConnectionString"] = Settings.CoreConnectionString,
            ["SecurityData__ConnectionString"] = Settings.SsoConnectionString,
            ["AppSecurity__ConnectionString"] = Settings.CoreConnectionString,
            ["Security__ConnectionString"] = Settings.SsoConnectionString,
            ["Security__DecryptionKey"] = Settings.DecryptionKey,
            ["AppSecurity__AggregateDomains"] = "false",
            ["ContentManagement__ConnectionString"] = Settings.CoreConnectionString,
            ["ContentManagement__WorkflowServiceUrl"] = WorkflowBaseAddress.ToString(),
            ["DocumentManagement__ConnectionString"] = Settings.CoreConnectionString,
            ["Logging__ConnectionString"] = Settings.CoreConnectionString,
            ["Mail__ConnectionString"] = Settings.CoreConnectionString,
            ["Packaging__ConnectionString"] = Settings.CoreConnectionString,
            ["Workflow__ConnectionString"] = Settings.CoreConnectionString,
            ["Workflow__ServiceUrl"] = WorkflowBaseAddress.ToString(),
            ["Workflow__SslPort"] = WebBaseAddress.Port.ToString(),
            ["Workflow__QueueInstanceManagement__PollingIntervalMilliseconds"] = "250",
            ["Eventing__ProviderType"] = Settings.EventProviderType,
            ["Eventing__Http__MaxConcurrency"] = "1"
        };

        environment["Mail__Providers__MicrosoftGraph__TenantId"] =
            Settings.MailTenantId;

        environment["Mail__Providers__MicrosoftGraph__ClientId"] =
            Settings.MailClientId;

        environment["Mail__Providers__MicrosoftGraph__ClientSecret"] =
            Settings.MailClientSecret;

        environment["CoreIntegrationTests__MailSendUser"] =
            Settings.MailSendUser;

        environment["CoreIntegrationTests__MailReceiveUser"] =
            Settings.MailReceiveUser;

        if (Settings.UseServiceBusEventing)
        {
            environment["Eventing__ServiceBus__ConnectionString"] =
                Settings.ServiceBusConnectionString;

            environment["Eventing__ServiceBus__MaxConcurrency"] =
                Settings.ServiceBusMaxConcurrency.ToString();
        }

        return environment;
    }

    private void AddHttpsCertificateEnvironment(Dictionary<string, string> environment)
    {
        string certificatePath = Path.Combine(path1: acceptanceArtifactsRoot, path2: "localhost-https.pfx");

        string certificatePassword = Guid.NewGuid()
            .ToString(format: "N");

        using RSA rsa = RSA.Create(keySizeInBits: 2048);

        CertificateRequest request = new(
            "CN=localhost",
            rsa,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);

        SubjectAlternativeNameBuilder subjectAlternativeNameBuilder = new();
        subjectAlternativeNameBuilder.AddDnsName(dnsName: "localhost");
        subjectAlternativeNameBuilder.AddIpAddress(ipAddress: IPAddress.Loopback);
        subjectAlternativeNameBuilder.AddIpAddress(ipAddress: IPAddress.IPv6Loopback);

        request.CertificateExtensions.Add(item: subjectAlternativeNameBuilder.Build());

        request.CertificateExtensions.Add(
item: new X509BasicConstraintsExtension(
                certificateAuthority: false,
                hasPathLengthConstraint: false,
                pathLengthConstraint: 0,
                critical: false));

        request.CertificateExtensions.Add(
item: new X509KeyUsageExtension(
                X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment,
                critical: true));

        request.CertificateExtensions.Add(
item: new X509EnhancedKeyUsageExtension(
                [new Oid("1.3.6.1.5.5.7.3.1")],
                critical: false));

        using X509Certificate2 certificate = request.CreateSelfSigned(
notBefore: DateTimeOffset.UtcNow.AddMinutes(minutes: -5), notAfter: DateTimeOffset.UtcNow.AddDays(days: 1));

        File.WriteAllBytes(path: certificatePath, bytes: certificate.Export(contentType: X509ContentType.Pkcs12, password: certificatePassword));

        environment["ASPNETCORE_Kestrel__Certificates__Default__Path"] = certificatePath;
        environment["ASPNETCORE_Kestrel__Certificates__Default__Password"] = certificatePassword;
    }

    private async Task EnsureServiceBusQueuesAreCleanAsync()
    {
        if (string.IsNullOrWhiteSpace(value: Settings.ServiceBusConnectionString))
        {
            throw new InvalidOperationException(
                "Service Bus integration mode requires Eventing__ServiceBus__ConnectionString.");
        }

        ServiceBusAdministrationClient administrationClient = new(Settings.ServiceBusConnectionString);

        foreach (string queueName in ServiceBusEventQueues)
        {
            if (!await administrationClient.QueueExistsAsync(name: queueName))
            {
                await administrationClient.CreateQueueAsync(name: queueName);
            }
        }

        await DrainServiceBusQueuesAsync();
    }

    private async Task DrainServiceBusQueuesAsync()
    {
        if (string.IsNullOrWhiteSpace(value: Settings?.ServiceBusConnectionString))
        {
            return;
        }

        await using ServiceBusClient client = new(Settings.ServiceBusConnectionString);

        foreach (string queueName in ServiceBusEventQueues)
        {
            ServiceBusReceiver receiver = client.CreateReceiver(queueName: queueName);

            while (true)
            {
                IReadOnlyList<ServiceBusReceivedMessage> messages =
                    await receiver.ReceiveMessagesAsync(maxMessages: 100, maxWaitTime: TimeSpan.FromSeconds(seconds: 1));

                if (messages.Count == 0)
                {
                    break;
                }

                foreach (ServiceBusReceivedMessage message in messages)
                {
                    await receiver.CompleteMessageAsync(message: message);
                }
            }

            await receiver.DisposeAsync();
        }
    }

    private static string ResolveFuncExecutablePath()
    {
        string bundledFuncExe = Path.Combine(
            paths:
            [
                Environment.GetFolderPath(folder: Environment.SpecialFolder.ApplicationData),
                "npm",
                "node_modules",
                "azure-functions-core-tools",
                "bin",
                "in-proc6",
                "func.exe"
            ]);

        if (File.Exists(path: bundledFuncExe))
        {
            return bundledFuncExe;
        }

        string fallbackFuncExe = Path.Combine(
            paths:
            [
                Environment.GetFolderPath(folder: Environment.SpecialFolder.ApplicationData),
                "npm",
                "node_modules",
                "azure-functions-core-tools",
                "bin",
                "func.exe"
            ]);

        if (File.Exists(path: fallbackFuncExe))
        {
            return fallbackFuncExe;
        }

        string roamingNpmFunc = Path.Combine(
path1: Environment.GetFolderPath(folder: Environment.SpecialFolder.ApplicationData), path2: "npm", path3: "func.cmd");

        if (File.Exists(path: roamingNpmFunc))
        {
            return roamingNpmFunc;
        }

        return "func";
    }
}