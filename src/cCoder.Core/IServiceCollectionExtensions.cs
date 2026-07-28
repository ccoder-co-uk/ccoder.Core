// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Models;
using cCoder.AppSecurity;
using cCoder.ContentManagement;
using cCoder.DocumentManagement;
using cCoder.Logging;
using cCoder.Mail;
using cCoder.Workflow;
using cCoder.Core.Brokers.AppSecurity;
using cCoder.Core.Brokers.ContentManagement;
using cCoder.Core.Brokers.DocumentManagement;
using cCoder.Core.Brokers.Eventing;
using cCoder.Core.Brokers.Http;
using cCoder.Core.Brokers.Mail;
using cCoder.Core.Brokers.Planning;
using cCoder.Core.Brokers.Packaging;
using cCoder.Core.Brokers.Workflow;
using cCoder.Core.Dependencies.Eventing;
using cCoder.Core.Dependencies.Packaging;
using cCoder.Core.Exposures.Managers;
using cCoder.Core.Exposures.Controllers;
using cCoder.Core.Exposures.Cors;
using cCoder.Core.Exposures.Setup;
using cCoder.Core.Dependencies.Formatters;
using cCoder.Core.Dependencies.Middleware;
using cCoder.Core.Dependencies.Sessions;
using cCoder.Core.Exposures;
using cCoder.Core.Services.Aggregations;
using cCoder.Core.Services.Aggregations.Packages;
using cCoder.Core.Services.Foundations.AllowedOrigins;
using cCoder.Core.Services.Foundations.AppSecurity;
using cCoder.Core.Services.Foundations.ContentManagement;
using cCoder.Core.Services.Foundations.DocumentManagement;
using cCoder.Core.Services.Foundations.Eventing;
using cCoder.Core.Services.Foundations.Mail;
using cCoder.Core.Services.Foundations.Planning;
using cCoder.Core.Services.Foundations.Workflow;
using cCoder.Core.Services.Orchestrations;
using cCoder.Core.Services.Processings.AllowedOrigins;
using cCoder.Core.Services.Processings.Setup;
using cCoder.Core.Services.Foundations.Setup;
using cCoder.Core.Services.Foundations.TemplatedEmails;
using cCoder.Core.Brokers.Setup;
using cCoder.Core.Services.Setup;
using cCoder.Data;
using cCoder.Data.Models.Packaging;
using cCoder.Eventing;
using cCoder.Eventing.Models;
using cCoder.Packaging;
using cCoder.Security.Objects.Events;
using cCoder.Security;
using cCoder.Security.Data.EF;
using cCoder.Security.Exposures;
using cCoder.Security.Services.Orchestrations.Interfaces;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Batch;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace cCoder.Core;

public static partial class IServiceCollectionExtensions
{
    public static void AddCoreWeb(
        IServiceCollection services,
        CoreConfiguration configuration) =>
        AddCoreWebServices(services, configuration);

    private static void AddCoreWebServices(
        IServiceCollection services,
        CoreConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        services.AddSingleton(configuration);
        ConfigureDefaultLogging(services, GetRequiredConfiguration(services));

        CoreApiBuilderOptions core = new(services);
        services.AddSecurityApi(configuration.Security);
        core.AddAppSecurityApi(configuration.AppSecurity);
        core.AddContentManagementApi(configuration.ContentManagement);
        core.AddDocumentManagementApi(configuration.DocumentManagement);
        core.AddLoggingApi(configuration.Logging);
        core.AddMailApi(configuration.Mail);
        core.AddWorkflowApi(configuration.Workflow);
        core.UseLegacyCoreApi();
        core.ConfigureEventing(configuration.Eventing);
        core.Apply();

        services.AddPackaging(configuration.Packaging);
        AddCoreFirstTimeSetup(services: services);
    }

    public static void AddCoreHostedServices(
        this IServiceCollection services,
        CoreConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        services.AddSingleton(configuration);
        ConfigureDefaultLogging(services, GetRequiredConfiguration(services));

        services.AddSecurityHostedServices(configuration.Security);
        services.AddAppSecurityHostedServices(configuration.AppSecurity);
        services.AddContentManagementHostedServices(configuration.ContentManagement);
        services.AddDocumentManagementHostedServices(configuration.DocumentManagement);
        services.AddLoggingHostedServices(configuration.Logging);
        services.AddMailHostedServices(configuration.Mail);
        services.AddWorkflowHostedServices(configuration.Workflow);
        services.AddPackaging(configuration.Packaging);

        CoreBuilderOptions core = new(services);
        core.ConfigureCoreServices(configuration);
        core.Apply();
        AddCoreAspNetExposures(services: services);
    }

    internal static IServiceCollection AddCoreApi(
        this IServiceCollection services,
        IEnumerable<CoreApiRouteDefinition> routeDefinitions = null) =>
        AddCoreExposures(services: services, routeDefinitions: routeDefinitions);

    internal static IServiceCollection AddCoreApiDocumentation(
        this IServiceCollection services,
        params string[] apiContexts)
    {
        CoreApiRouteDefinition[] routes = GetRouteDefinitions(apiContexts: apiContexts);
        return services.AddCoreApiDocumentation(routes: routes);
    }

    internal static IServiceCollection AddCoreApiDocumentation(
        this IServiceCollection services,
        IEnumerable<CoreApiRouteDefinition> routes)
    {
        CoreApiRouteDefinition[] definitions = GetRouteDefinitions(routes: routes);

        services.AddSwaggerGen(setupAction: c =>
        {
            c.ResolveConflictingActions(resolver: apiDescriptions => apiDescriptions.First());
            c.CustomSchemaIds(schemaIdSelector: type => type.FullName?.Replace(oldChar: '+', newChar: '.') ?? type.Name);
            AddSwaggerDocuments(options: c, routes: definitions);

            c.DocInclusionPredicate(
predicate: (documentName, apiDescription) =>
                    ShouldIncludeInDocument(documentName: documentName, relativePath: apiDescription.RelativePath, routes: definitions));

            c.AddSecurityDefinition(name: "bearer", securityScheme: new OpenApiSecurityScheme
            {
                Description = @"Authorization header using the Bearer scheme. \r\n\r\n 
                        Enter 'Bearer' [space] and then your token in the text input below.
                        \r\n\r\nExample: 'bearer 12345abcdef'",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "bearer",
            });
        });

        return services;
    }

    internal static void AddCoreEventing(
        this IServiceCollection services,
        IEnumerable<EventProvider> eventProviders)
    {
        services.AddEventing(configure: configuration =>
        {
            configuration.EventProviders =
                [.. (eventProviders ?? []).Where(predicate: provider => provider is not null)];
        });

        services.AddEventingForType<SecurityAccountEvent>();
    }

    private static void AddCoreApi(
        IServiceCollection services,
        Action<CoreApiBuilderOptions> setupAction)
    {
        CoreApiBuilderOptions config = new(services);
        setupAction(obj: config);
        config.Apply();
    }

    private static void AddCore(
        IServiceCollection services,
        Action<CoreBuilderOptions> setupAction)
    {
        CoreBuilderOptions config = new(services);
        setupAction(obj: config);
        config.Apply();
    }

    private static void AddCoreBrokers(IServiceCollection services)
    {
        services.AddTransient<IContentManagementAppBroker, ContentManagementAppBroker>();
        services.AddTransient<IAppGraphEventBroker, AppGraphEventBroker>();
        services.AddTransient<IAuthInfoBroker, AuthInfoBroker>();
        services.AddTransient<IHttpRequestBroker, HttpRequestBroker>();
        services.AddTransient<IAppSecurityAppBroker, AppSecurityAppBroker>();
        services.AddTransient<IPlanningAppBroker, PlanningAppBroker>();
        services.AddTransient<IDocumentManagementAppBroker, DocumentManagementAppBroker>();

        services.AddTransient<IWorkflowAppBroker, WorkflowAppBroker>();
        services.AddTransient<IMailAppBroker, MailAppBroker>();
        services.AddTransient<IMailManagerBroker, MailManagerBroker>();
        services.TryAddTransient<cCoder.Packaging.Brokers.IAppDomainProvider, AppDomainProvider>();
        services.TryAddTransient<cCoder.Packaging.Brokers.IAppSecurityPackageManagerBroker, AppSecurityPackageManagerBroker>();
        services.TryAddTransient<
            cCoder.Packaging.Brokers.IContentManagementPackageManagerBroker,
            ContentManagementPackageManagerBroker>();

        services.TryAddTransient<
            cCoder.Packaging.Brokers.IDocumentManagementPackageManagerBroker,
            DocumentManagementPackageManagerBroker>();

        services.TryAddTransient<cCoder.Packaging.Brokers.ISchedulingPackageManagerBroker, SchedulingPackageManagerBroker>();
        services.TryAddTransient<cCoder.Packaging.Brokers.IWorkflowPackageManagerBroker, WorkflowPackageManagerBroker>();
        services.AddPackaging();
    }

    private static void AddCoreFoundationServices(IServiceCollection services)
    {
        services.AddTransient<IContentManagementAppService, ContentManagementAppService>();
        services.AddTransient<IAppGraphEventService, AppGraphEventService>();
        services.AddTransient<IAllowedOriginStoreService, AllowedOriginStoreService>();
        services.AddTransient<IAppSecurityAppService, AppSecurityAppService>();
        services.AddTransient<IPlanningAppService, PlanningAppService>();
        services.AddTransient<IDocumentManagementAppService, DocumentManagementAppService>();
        services.AddTransient<IWorkflowAppService, WorkflowAppService>();
        services.AddTransient<IMailAppService, MailAppService>();
        services.AddTransient<IMailManagerService, MailManagerService>();
        services.AddTransient<IPackageManagerDependency, PackageManagerDependency>();
        services.AddTransient<IPackageBroker, PackageBroker>();
    }

    private static void AddCoreProcessingServices(IServiceCollection services) =>
        services.AddTransient<
            IAllowedOriginStoreProcessingService,
            AllowedOriginStoreProcessingService>();

    private static void AddCoreOrchestrationServices(IServiceCollection services)
    {
        services.AddTransient<IAppAggregationService, AppAggregationService>();
        services.AddTransient<IAppOrchestrationService, AppAggregationService>();
        services.AddTransient<
            ITemplatedEmailContentService,
            TemplatedEmailContentService>();

        services.AddTransient<
            ITemplatedEmailIdentityService,
            TemplatedEmailIdentityService>();

        services.AddTransient<
            ITemplatedEmailQueueService,
            TemplatedEmailQueueService>();

        services.AddTransient<
            ITemplatedEmailOperationOrchestrationService,
            TemplatedEmailOperationOrchestrationService>();
        services.AddTransient<ITemplatedEmailManager, TemplatedEmailManager>();
        services.AddTransient<ITemplatedEmailOrchestrationService, TemplatedEmailManager>();
        services.AddTransient<IUserRegistrationOrchestrationService, UserRegistrationManager>();
        services.AddTransient<IUserRegistrationAggregationService, UserRegistrationAggregationService>();

        services.AddTransient<
            IPackageManagerAggregationService,
            PackageManagerAggregationService>();

        services.AddTransient<
            ISecurityAccountEmailAggregationService,
            SecurityAccountEmailAggregationService>();
    }

    private static void AddCoreFirstTimeSetup(IServiceCollection services)
    {
        EnsureFirstTimeSetupSecurityServices(services: services);
        EnsureFirstTimeSetupSecurityManagers(services: services);
        services.AddScoped<ICoreSetupContextBroker, CoreSetupContextBroker>();
        services.AddScoped<ISecuritySetupContextBroker, SecuritySetupContextBroker>();
        services.AddScoped<ICoreSetupStateService, CoreSetupStateService>();
        services.AddScoped<ISecuritySetupStateService, SecuritySetupStateService>();
        services.AddScoped<IAppSecurityAppService, AppSecurityAppService>();
        services.AddScoped<IAppSecurityUserRoleService, AppSecurityUserRoleService>();
        services.AddScoped<
            IFirstTimeSetupStateOrchestrationService,
            FirstTimeSetupStateOrchestrationService>();
        services.AddScoped<IFirstTimeSetupStateService, FirstTimeSetupStateManager>();
        services.AddScoped<ISetupRequestHostProcessingService, SetupRequestHostProcessingService>();
        services.AddScoped<ISetupRequestHostManager, SetupRequestHostManager>();
        IMvcBuilder mvcBuilder = services.AddMvc();

        mvcBuilder.AddApplicationPart(
            assembly: typeof(SetupController).Assembly);
    }

    private static void EnsureFirstTimeSetupSecurityServices(IServiceCollection services)
    {
        if (HasServiceRegistration(
                services: services,
                assemblyQualifiedTypeName:
                    "cCoder.Security.Services.Orchestrations.Interfaces.IAuthenticationOrchestrationService, cCoder.Security")
            && HasServiceRegistration(
                services: services,
                assemblyQualifiedTypeName:
                    "cCoder.Security.Services.Foundations.Events.ITenantSetupEventService, cCoder.Security"))
        {
            return;
        }

        CoreConfiguration coreConfiguration = services
            .Where(predicate: descriptor => descriptor.ServiceType == typeof(CoreConfiguration))
            .Select(selector: descriptor => descriptor.ImplementationInstance)
            .OfType<CoreConfiguration>()
            .LastOrDefault();

        string securityConnectionString =
            coreConfiguration?.Security.ConnectionString ?? string.Empty;

        string decryptionKey =
            coreConfiguration?.Security.DecryptionKey ?? string.Empty;

        cCoder.Security.IServiceCollectionExtensions.AddSecurity(
            services: services,
            configAction: (securityServices, securityConfig) =>
            {
                securityConfig.RootPath = null;
                securityConfig.ConnectionString =
                    securityConnectionString ?? string.Empty;

                securityConfig.UseAESHMMACPasswordEncryption(
                    services: securityServices,
                    decryptionKey: decryptionKey ?? string.Empty);
            });
    }

    private static void EnsureFirstTimeSetupSecurityManagers(
        IServiceCollection services)
    {
        if (!services.Any(predicate: descriptor =>
                descriptor.ServiceType == typeof(ITokenManager)))
        {
            Type tokenManagerType = Type.GetType(
                typeName:
                    "cCoder.Security.Exposures.TokenManager, cCoder.Security");

            if (tokenManagerType is not null)
            {
                services.AddTransient(
                    serviceType: typeof(ITokenManager),
                    implementationType: tokenManagerType);
            }
        }

        if (!services.Any(predicate: descriptor =>
                descriptor.ServiceType == typeof(ITenantManager)))
        {
            Type tenantManagerType = Type.GetType(
                typeName:
                    "cCoder.Security.Exposures.TenantManager, cCoder.Security");

            if (tenantManagerType is not null)
            {
                services.AddTransient(
                    serviceType: typeof(ITenantManager),
                    implementationType: tenantManagerType);
            }
        }
    }

    private static bool HasServiceRegistration(
        IServiceCollection services,
        string assemblyQualifiedTypeName)
    {
        Type serviceType = Type.GetType(
            typeName: assemblyQualifiedTypeName);

        string fullName =
            assemblyQualifiedTypeName.Split(separator: ',')[0];

        return services.Any(predicate: descriptor =>
            descriptor.ServiceType == serviceType
            || string.Equals(
                a: descriptor.ServiceType.FullName,
                b: fullName,
                comparisonType: StringComparison.Ordinal));
    }

    private static IServiceCollection AddCoreExposures(
        IServiceCollection services,
        IEnumerable<CoreApiRouteDefinition> routeDefinitions = null)
    {
        AddCoreAspNetExposures(services: services);
        AddCoreBrokers(services: services);
        AddCoreFoundationServices(services: services);
        AddCoreProcessingServices(services: services);
        AddCoreOrchestrationServices(services: services);
        services.AddScoped<ICoreAllowedOriginStore, CoreAllowedOriginStore>();

        AddCoreODataExposures(
            services: services,
            routeDefinitions: routeDefinitions);

        AddCoreODataRouteMode(services: services);

        return services;
    }

    private static void AddCoreAspNetExposures(IServiceCollection services)
    {
        CoreConfiguration coreConfiguration =
            GetRegisteredCoreConfiguration(services: services);

        services.AddRouting();
        services.AddResponseCompression();
        services.AddHttpClient();
        services.AddHttpContextAccessor();
        services.AddTransient<CoreFormatterMiddleware>();
        services.AddTransient<CoreExceptionMiddleware>();

        services.AddScoped(
            serviceType: typeof(HttpContext),
            implementationFactory: context =>
                CreateHttpContext(
                    httpContext: context
                        .GetService<IHttpContextAccessor>()
                        ?.HttpContext));

        services.AddScoped(
            serviceType: typeof(HttpRequest),
            implementationFactory: context =>
                context.GetRequiredService<HttpContext>().Request);

        services.AddScoped(
            serviceType: typeof(ISession),
            implementationFactory: context =>
            {
                HttpContext httpContext =
                    context.GetRequiredService<HttpContext>();

                return httpContext.Features
                    .Get<ISessionFeature>()
                    ?.Session
                    ?? NoOpSession.Instance;
            });

        services.AddSession(configure: options =>
        {
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy =
                CookieSecurePolicy.Always;
        });

        services.AddHsts(configureOptions: options =>
        {
            options.Preload = true;
            options.IncludeSubDomains = true;
            options.MaxAge = TimeSpan.FromMinutes(minutes: 60);
        });

        services.AddMvc(setupAction: options =>
        {
            options.EnableEndpointRouting = false;
            options.OutputFormatters.Add(item: new XmlFormatter());
            options.OutputFormatters.Add(item: new CsvFormatter());
            options.OutputFormatters.Add(item: new ExcelFormatter());

            if (coreConfiguration?.AppSecurity.AggregateDomains != true)
            {
                options.Conventions.Add(
                    actionModelConvention:
                        new SplitDomainApplicationModelConvention());
            }
        });

        services.AddRazorPages();

        services.Configure<KestrelServerOptions>(
            configureOptions: options =>
            {
                options.Limits.MaxRequestBodySize = int.MaxValue;
            });

        services.AddEndpointsApiExplorer();
        services.AddSignalR();
    }

    private static void AddCoreODataExposures(
        IServiceCollection services,
        IEnumerable<CoreApiRouteDefinition> routeDefinitions)
    {
        DefaultODataBatchHandler batchHandler = new();
        ODataConventionModelBuilder packagingModelBuilder = new();

        packagingModelBuilder.EntitySet<Package>(name: nameof(Package));
        packagingModelBuilder.EntitySet<PackageItem>(name: nameof(PackageItem));
        packagingModelBuilder.Namespace = string.Empty;

        IEdmModel packagingModel = packagingModelBuilder.GetEdmModel();

        CoreApiRouteDefinition[] definitions = [.. (routeDefinitions ?? [])
            .Where(predicate: route =>
                route is not null
                && (string.Equals(
                        a: route.Name,
                        b: "Core",
                        comparisonType: StringComparison.OrdinalIgnoreCase)
                    || string.Equals(
                        a: route.RoutePath,
                        b: "Api/Core",
                        comparisonType: StringComparison.OrdinalIgnoreCase)
                    || string.Equals(
                        a: route.Name,
                        b: "Security",
                        comparisonType: StringComparison.OrdinalIgnoreCase)
                    || string.Equals(
                        a: route.RoutePath,
                        b: "Api/Security",
                        comparisonType: StringComparison.OrdinalIgnoreCase)))];

        IMvcBuilder mvcBuilder = services.AddControllers();

        mvcBuilder.AddOData(setupAction: options =>
        {
            options.RouteOptions.EnableQualifiedOperationCall = false;
            options.EnableAttributeRouting = true;
            options.RouteOptions.EnableKeyAsSegment = false;

            options.Expand()
                .Count()
                .Filter()
                .Select()
                .OrderBy()
                .SetMaxTop(maxTopValue: 1000);

            _ = options.AddRouteComponents(
                routePrefix: "Api/Packaging",
                model: packagingModel,
                batchHandler: batchHandler);

            foreach (CoreApiRouteDefinition routeDefinition in definitions)
            {
                _ = options.AddRouteComponents(
                    routePrefix: routeDefinition.RoutePath,
                    model: routeDefinition.RouteModel,
                    batchHandler: batchHandler);
            }
        });
    }

    private static void AddCoreODataRouteMode(
        IServiceCollection services)
    {
        CoreConfiguration coreConfiguration =
            GetRegisteredCoreConfiguration(services: services);

        services.PostConfigure<ODataOptions>(
            configureOptions: options =>
            {
                if (coreConfiguration?.AppSecurity.AggregateDomains != true)
                {
                    return;
                }

                string[] aggregateDomainRoutes =
                [
                    "Api/AppSecurity",
                    "Api/ContentManagement",
                    "Api/DocumentManagement",
                    "Api/Logging",
                    "Api/Mail",
                    "Api/Workflow",
                ];

                foreach (string route in aggregateDomainRoutes)
                {
                    options.RouteComponents.Remove(key: route);
                }
            });
    }

    private static CoreConfiguration GetRegisteredCoreConfiguration(
        IServiceCollection services) =>
        services
            .Where(predicate: descriptor =>
                descriptor.ServiceType == typeof(CoreConfiguration))
            .Select(selector: descriptor =>
                descriptor.ImplementationInstance)
            .OfType<CoreConfiguration>()
            .LastOrDefault();

    private static HttpContext CreateHttpContext(
        HttpContext httpContext)
    {
        if (httpContext is not null)
        {
            return httpContext;
        }

        DefaultHttpContext fallbackContext = new();

        fallbackContext.Features.Set<ISessionFeature>(
            instance: new NoOpSessionFeature());

        return fallbackContext;
    }

    private sealed class SplitDomainApplicationModelConvention
        : IActionModelConvention
    {
        public void Apply(ActionModel action)
        {
            if (!string.Equals(
                    a: action.Controller.ControllerName,
                    b: "App",
                    comparisonType: StringComparison.Ordinal))
            {
                return;
            }

            for (int index = action.Selectors.Count - 1; index >= 0; index--)
            {
                string template =
                    action.Selectors[index].AttributeRouteModel?.Template;

                if (template?.StartsWith(
                        value: "Api/Core/App",
                        comparisonType:
                            StringComparison.OrdinalIgnoreCase) == true)
                {
                    action.Selectors.RemoveAt(index: index);
                }
            }
        }
    }

    private static IConfiguration GetRequiredConfiguration(IServiceCollection services)
    {
        IConfiguration configuration = services
            .Where(predicate: descriptor => typeof(IConfiguration).IsAssignableFrom(c: descriptor.ServiceType))
            .Select(selector: descriptor => descriptor.ImplementationInstance)
            .OfType<IConfiguration>()
            .LastOrDefault();

        if (configuration is not null)
        {
            return configuration;
        }

        ServiceProvider serviceProvider = services.BuildServiceProvider();

        return serviceProvider.GetService<IConfiguration>()
            ?? throw new InvalidOperationException(
                "IConfiguration must already be registered on the IServiceCollection before calling AddCoreWeb or AddCoreHostedServices.");
    }

    private static void ConfigureDefaultLogging(
        IServiceCollection services,
        IConfiguration configuration) =>
        services.AddLogging(configure: logBuilder =>
        {
            logBuilder.ClearProviders();
            logBuilder.AddFilter(levelFilter: level => level >= LogLevel.Debug);

            logBuilder.AddSimpleConsole(configure: options =>
            {
                options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss ";
                options.SingleLine = true;
            });

            logBuilder.AddConfiguration(configuration: configuration.GetSection(key: "Logging"));
        });

    private static CoreApiRouteDefinition[] GetRouteDefinitions(IEnumerable<string> apiContexts) =>
        GetRouteDefinitions(routes: (apiContexts ?? [])
            .Where(predicate: context => !string.IsNullOrWhiteSpace(value: context))
            .Select(selector: context => new CoreApiRouteDefinition(
                context,
                $"Api/{context}",
                null)));

    private static CoreApiRouteDefinition[] GetRouteDefinitions(
        IEnumerable<CoreApiRouteDefinition> routes)
    {
        CoreApiRouteDefinition coreRoute = new("Core", "Api/Core", null);

        return [coreRoute, .. (routes ?? [])
            .Where(predicate: route => route is not null && !string.IsNullOrWhiteSpace(value: route.Name))
            .GroupBy(keySelector: route => route.Name,comparer: StringComparer.OrdinalIgnoreCase)
            .Select(selector: group => group.First())
            .Where(predicate: route => !string.Equals(a: route.Name,b: "Core",comparisonType: StringComparison.OrdinalIgnoreCase))];
    }

    private static void AddSwaggerDocuments(
        Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions options,
        IEnumerable<CoreApiRouteDefinition> routes)
    {
        foreach (CoreApiRouteDefinition route in routes)
        {
            options.SwaggerDoc(name: route.Name, info: new OpenApiInfo
            {
                Title = $"{route.Name} API definition",
                Version = route.Name,
            });
        }

        options.SwaggerDoc(name: "v1", info: new OpenApiInfo
        {
            Title = "Corporate LinX V7 API definition",
            Version = "v1",
        });
    }

    private static bool ShouldIncludeInDocument(
        string documentName,
        string relativePath,
        IEnumerable<CoreApiRouteDefinition> routes)
    {
        if (string.IsNullOrWhiteSpace(value: relativePath))
        {
            return false;
        }

        if (string.Equals(a: documentName, b: "v1", comparisonType: StringComparison.OrdinalIgnoreCase))
        {
            documentName = "Core";
        }

        string path = NormalizePath(relativePath: relativePath);

        if (string.Equals(a: documentName, b: "Core", comparisonType: StringComparison.OrdinalIgnoreCase))
        {
            return IsCoreRoute(path: path, routes: routes);
        }

        CoreApiRouteDefinition route = routes.FirstOrDefault(predicate: candidate =>
            string.Equals(a: candidate.Name, b: documentName, comparisonType: StringComparison.OrdinalIgnoreCase));

        return route is not null && MatchesRoutePath(path: path, routePath: route.RoutePath);
    }

    private static bool IsCoreRoute(string path, IEnumerable<CoreApiRouteDefinition> routes)
    {
        if (MatchesContextRoute(path: path, context: "Core"))
        {
            return true;
        }

        if (!path.Equals(value: "/Api", comparisonType: StringComparison.OrdinalIgnoreCase)
            && !path.StartsWith(value: "/Api/", comparisonType: StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        foreach (CoreApiRouteDefinition route in routes.Where(predicate: route =>
                     !string.Equals(a: route.Name, b: "Core", comparisonType: StringComparison.OrdinalIgnoreCase)
                     && !string.Equals(a: route.Name, b: "v1", comparisonType: StringComparison.OrdinalIgnoreCase)))
        {
            if (MatchesRoutePath(path: path, routePath: route.RoutePath))
            {
                return false;
            }
        }

        return true;
    }

    private static bool MatchesRoutePath(string path, string routePath)
    {
        string prefix = NormalizePath(relativePath: routePath);

        return path.Equals(value: prefix, comparisonType: StringComparison.OrdinalIgnoreCase)
            || path.StartsWith(value: $"{prefix}/", comparisonType: StringComparison.OrdinalIgnoreCase);
    }

    private static bool MatchesContextRoute(string path, string context)
    {
        string prefix = $"/Api/{context}";

        return path.Equals(value: prefix, comparisonType: StringComparison.OrdinalIgnoreCase)
            || path.StartsWith(value: $"{prefix}/", comparisonType: StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizePath(string relativePath) =>
        relativePath.StartsWith(value: '/') ? relativePath : $"/{relativePath}";
}